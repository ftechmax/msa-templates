using ApplicationName.Worker.Application;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace ApplicationName.Worker.Infrastructure;

public class CacheInvalidationService(
    IConnectionMultiplexer connectionMultiplexer,
    IServiceProvider serviceProvider,
    IMapper mapper,
    ILogger<CacheInvalidationService> logger) : BackgroundService
{
    private const string ProjectionPrefix = "projections:";

    private ISubscriber _sub = null!;

    private readonly ConcurrentDictionary<string, byte> _queuedOrRunning = new(StringComparer.Ordinal);

    private readonly Channel<string> _queue = Channel.CreateBounded<string>(new BoundedChannelOptions(10_000)
    {
        SingleReader = true,
        SingleWriter = false,
        FullMode = BoundedChannelFullMode.DropOldest
    });

    private readonly Dictionary<string, CacheProjectionMapping> _mappingsByPrefix =
        CacheProjectionRegistry.Mappings.ToDictionary(m => m.CacheKeyPrefix, StringComparer.Ordinal);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _sub = connectionMultiplexer.GetSubscriber();

            await _sub.SubscribeAsync(RedisChannel.Literal("__keyevent@0__:del"), OnKeyEvent);
            await _sub.SubscribeAsync(RedisChannel.Literal("__keyevent@0__:expired"), OnKeyEvent);

            await InitializeProjectionsAsync(stoppingToken);

            logger.LogInformation("Cache invalidation service started. Listening for DEL/EXPIRED under '{Prefix}'.", ProjectionPrefix);

            await ProcessQueueAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // normal shutdown
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to start cache invalidation service");
            throw;
        }
    }

    private void OnKeyEvent(RedisChannel channel, RedisValue keyValue)
    {
        var key = keyValue.ToString();

        if (!key.StartsWith(ProjectionPrefix, StringComparison.Ordinal))
        {
            return;
        }

        if (!_queuedOrRunning.TryAdd(key, 0))
        {
            return;
        }

        if (!_queue.Writer.TryWrite(key))
        {
            _queuedOrRunning.TryRemove(key, out _);
        }
    }

    private async Task ProcessQueueAsync(CancellationToken stoppingToken)
    {
        await foreach (var key in _queue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await RebuildSingleProjectionAsync(key, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error rebuilding projection {Key}", key);
            }
            finally
            {
                _queuedOrRunning.TryRemove(key, out _);
            }
        }
    }

    private async Task InitializeProjectionsAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Initializing projections for all registered document types...");

        var totalCount = 0;

        foreach (var mapping in CacheProjectionRegistry.Mappings)
        {
            stoppingToken.ThrowIfCancellationRequested();
            totalCount += await RebuildProjectionsForDocumentTypeAsync(mapping, stoppingToken);
        }

        logger.LogInformation("Initialized {Count} projections", totalCount);
    }

    private async Task<int> RebuildProjectionsForDocumentTypeAsync(CacheProjectionMapping mapping, CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
        var documentRepository = scope.ServiceProvider.GetRequiredService<IDocumentRepository>();
        var protoCacheRepository = scope.ServiceProvider.GetRequiredService<IProtoCacheRepository>();

        var documents = await documentRepository.GetAllByTypeAsync(mapping.DocumentType);

        var count = 0;
        foreach (var document in documents)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var projection = mapper.Map(document, mapping.DocumentType, mapping.ProjectionType);
            var cacheKey = $"{ProjectionPrefix}{mapping.CacheKeyPrefix}:{document.Id:N}";

            await protoCacheRepository.SetAsync(cacheKey, projection); ////, mapping.ProjectionType);
            count++;
        }

        logger.LogInformation("Built {Count} for {DocumentType}", count, mapping.DocumentType.Name);
        return count;
    }

    private async Task RebuildSingleProjectionAsync(string cacheKey, CancellationToken stoppingToken)
    {
        if (!TryParseProjectionKey(cacheKey, out var cacheKeyPrefix, out var id))
        {
            return;
        }

        if (!_mappingsByPrefix.TryGetValue(cacheKeyPrefix, out var mapping))
        {
            return;
        }

        using var scope = serviceProvider.CreateScope();
        var documentRepository = scope.ServiceProvider.GetRequiredService<IDocumentRepository>();
        var protoCacheRepository = scope.ServiceProvider.GetRequiredService<IProtoCacheRepository>();

        var document = await documentRepository.GetByIdAndTypeAsync(id, mapping.DocumentType);
        if (document is null)
        {
            await protoCacheRepository.RemoveAsync(cacheKey);
            logger.LogInformation("Removed stale projection key {Key}", cacheKey);
            return;
        }

        stoppingToken.ThrowIfCancellationRequested();

        var projection = mapper.Map(document, mapping.DocumentType, mapping.ProjectionType);
        await protoCacheRepository.SetAsync(cacheKey, projection); ////, mapping.ProjectionType);

        logger.LogInformation("Rebuilt projection {Key}", cacheKey);
    }

    private static bool TryParseProjectionKey(string key, out string cacheKeyPrefix, out Guid id)
    {
        cacheKeyPrefix = "";
        id = Guid.Empty;

        if (!key.StartsWith(ProjectionPrefix, StringComparison.Ordinal))
        {
            return false;
        }

        // projections:{prefix}:{guid}
        var rest = key.AsSpan(ProjectionPrefix.Length);
        var colon = rest.IndexOf(':');
        if (colon <= 0) return false;

        cacheKeyPrefix = rest[..colon].ToString();
        return Guid.TryParse(rest[(colon + 1)..], out id);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Cache invalidation service stopping...");

        _queue.Writer.TryComplete();

        if (_sub != null)
        {
            try
            {
                await _sub.UnsubscribeAsync(RedisChannel.Literal("__keyevent@0__:del"));
                await _sub.UnsubscribeAsync(RedisChannel.Literal("__keyevent@0__:expired"));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error unsubscribing");
            }
        }

        await base.StopAsync(cancellationToken);
    }
}
