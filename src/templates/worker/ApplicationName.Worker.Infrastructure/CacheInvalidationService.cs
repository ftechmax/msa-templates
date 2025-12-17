using ApplicationName.Worker.Application;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Text.Json;

namespace ApplicationName.Worker.Infrastructure;

public class CacheInvalidationService(
    IConnectionMultiplexer connectionMultiplexer,
    IServiceProvider serviceProvider,
    IMapper mapper,
    ILogger<CacheInvalidationService> logger) : BackgroundService
{
    private IDatabase _db = null!;
    private readonly string _projectionPrefix = "projections:";
    private readonly ConcurrentDictionary<string, byte> _inflight = new();
    private ISubscriber _sub = null!;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _db = connectionMultiplexer.GetDatabase();
            _sub = connectionMultiplexer.GetSubscriber();

            var clientId = (long)await _db.ExecuteAsync("CLIENT", "ID");

            await _sub.SubscribeAsync(RedisChannel.Literal("__redis__:invalidate"), OnInvalidateMessage);
            await _sub.SubscribeAsync(RedisChannel.Literal("__keyevent@0__:del"), OnKeyEvent);
            await _sub.SubscribeAsync(RedisChannel.Literal("__keyevent@0__:expired"), OnKeyEvent);

            await _db.ExecuteAsync(
                "CLIENT", "TRACKING", "ON",
                "BCAST",
                "PREFIX", _projectionPrefix,
                "REDIRECT", clientId,
                "NOLOOP"
            );

            // build all projections
            await InitializeProjectionsAsync();

            logger.LogInformation("Cache invalidation service started. Tracking '{Prefix}' keys.", _projectionPrefix);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // shutdown
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to start cache invalidation service");
            throw;
        }
    }

    private async Task InitializeProjectionsAsync()
    {
        try
        {
            logger.LogInformation("Initializing projections for all registered document types...");

            var totalCount = 0;

            foreach (var mapping in CacheProjectionRegistry.Mappings)
            {
                totalCount += await RebuildProjectionsForDocumentTypeAsync(mapping);
            }

            logger.LogInformation("Initialized {Count} projections", totalCount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initialize projections");
            throw;
        }
    }

    private async Task<int> RebuildProjectionsForDocumentTypeAsync(CacheProjectionMapping mapping)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var documentRepository = scope.ServiceProvider.GetRequiredService<IDocumentRepository>();
            var protoCacheRepository = scope.ServiceProvider.GetRequiredService<IProtoCacheRepository>();

            var documents = await documentRepository.GetAllByTypeAsync(mapping.DocumentType);

            var count = 0;
            foreach (var document in documents)
            {
                var projection = mapper.Map(document, mapping.DocumentType, mapping.ProjectionType);
                var cacheKey = $"projections:{mapping.CacheKeyPrefix}:{document.Id:N}";

                var setAsyncMethod = typeof(IProtoCacheRepository)
                    .GetMethod(nameof(IProtoCacheRepository.SetAsync))!
                    .MakeGenericMethod(mapping.ProjectionType);

                await ((Task)setAsyncMethod.Invoke(protoCacheRepository, [cacheKey, projection, null])!);
                count++;
            }

            logger.LogInformation("Built {Count} for {DocumentType}", count, mapping.DocumentType.Name);
            return count;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to build projections for {DocumentType}", mapping.DocumentType.Name);
            throw;
        }
    }

    private async Task RebuildSingleProjectionAsync(string cacheKey)
    {
        try
        {
            if (!cacheKey.StartsWith(_projectionPrefix, StringComparison.Ordinal))
            { 
                return;
            }

            var parts = cacheKey.Split(':', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3) // projections, prefix, guid
            { 
                return;
            }

            var cacheKeyPrefix = parts[1];
            var idPart = parts[2];
            if (!Guid.TryParse(idPart, out var id)) 
            { 
                return; 
            }

            var mapping = CacheProjectionRegistry.Mappings.FirstOrDefault(i => string.Equals(i.CacheKeyPrefix, cacheKeyPrefix, StringComparison.Ordinal));
            if (mapping == null)
            { 
                return; 
            }

            using var scope = serviceProvider.CreateScope();
            var documentRepository = scope.ServiceProvider.GetRequiredService<IDocumentRepository>();
            var protoCacheRepository = scope.ServiceProvider.GetRequiredService<IProtoCacheRepository>();

            var document = await documentRepository.GetByIdAndTypeAsync(id, mapping.DocumentType);
            if (document == null)
            {
                await protoCacheRepository.RemoveAsync(cacheKey);
                logger.LogInformation("Removed stale projection key {Key}", cacheKey);
                return;
            }

            var projection = mapper.Map(document, mapping.DocumentType, mapping.ProjectionType);
            var setAsyncMethod = typeof(IProtoCacheRepository)
                .GetMethod(nameof(IProtoCacheRepository.SetAsync))!
                .MakeGenericMethod(mapping.ProjectionType);

            await (Task)setAsyncMethod.Invoke(protoCacheRepository, [cacheKey, projection, null])!;

            logger.LogInformation("Rebuilt projection {Key}", cacheKey);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error rebuilding projection {Key}", cacheKey);
        }
    }

    private void OnInvalidateMessage(RedisChannel channel, RedisValue value)
    {
        try
        {
            var keys = ParseInvalidationPayload(value).Where(i => i.StartsWith(_projectionPrefix, StringComparison.Ordinal)).Distinct();
            if (!keys.Any())
            { 
                return;
            }

            foreach (var key in keys)
            {
                // Debounce: avoid duplicate rebuild within a very short window
                if (!_inflight.TryAdd(key, 0))
                { 
                    continue;
                }

                _ = Task.Run(async () =>
                {
                    await RebuildSingleProjectionAsync(key);
                    _inflight.TryRemove(key, out _);
                });
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed processing invalidate payload");
        }
    }

    private void OnKeyEvent(RedisChannel channel, RedisValue keyValue)
    {
        try
        {
            var key = keyValue.ToString();
            if (!key.StartsWith(_projectionPrefix, StringComparison.Ordinal))
            {
                return;
            }

            if (!_inflight.TryAdd(key, 0))
            { 
                return; 
            }

            _ = Task.Run(async () =>
            {
                await RebuildSingleProjectionAsync(key);
                _inflight.TryRemove(key, out _);
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling keyevent {Channel}", channel.ToString());
        }
    }

    private static IEnumerable<string> ParseInvalidationPayload(RedisValue value)
    {
        var raw = value.ToString();
        if (string.IsNullOrWhiteSpace(raw))
        { 
            yield break;
        }

        if (raw.StartsWith("["))
        {
            string[]? arr = null;
            try { 
                arr = JsonSerializer.Deserialize<string[]>(raw); 
            } 
            catch
            { 
                arr = null;
            }

            if (arr != null)
            {
                foreach (var k in arr)
                {
                    if (!string.IsNullOrWhiteSpace(k))
                    { 
                        yield return k.Trim();
                    }
                }
                yield break;
            }
        }

        foreach (var part in raw.Split([' ', '\n', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries))
        {
            var cleaned = part.Trim().Trim('"', ',', '[', ']');
            if (!string.IsNullOrWhiteSpace(cleaned))
            { 
                yield return cleaned;
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Cache invalidation service stopping...");
        if (_db != null)
        {
            try
            { 
                await _db.ExecuteAsync("CLIENT", "TRACKING", "OFF");
            }
            catch (Exception ex)
            { 
                logger.LogError(ex, "Error disabling tracking");
            }
        }

        if (_sub != null)
        {
            try
            {
                await _sub.UnsubscribeAsync(RedisChannel.Literal("__redis__:invalidate"));
                await _sub.UnsubscribeAsync(RedisChannel.Literal("__keyevent@0__:del"));
                await _sub.UnsubscribeAsync(RedisChannel.Literal("__keyevent@0__:expired"));
            }
            catch (Exception ex) { logger.LogError(ex, "Error unsubscribing"); }
        }
        await base.StopAsync(cancellationToken);
    }
}