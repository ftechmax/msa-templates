using ApplicationName.Worker.Application.Documents;
using ApplicationName.Worker.Contracts;
using ApplicationName.Shared.Projections;
using ArgDefender;
using MapsterMapper;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace ApplicationName.Worker.Application.Services;

public class CacheInvalidationService(
    IConnectionMultiplexer connectionMultiplexer,
    IDocumentRepository documentRepository,
    IMapper mapper,
    ILogger<CacheInvalidationService> logger) : BackgroundService
{
    private ISubscriber _subscriber = null!;
    private IDatabase _database = null!;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _subscriber = connectionMultiplexer.GetSubscriber();
        _database = connectionMultiplexer.GetDatabase();

        try
        {
            // Enable keyspace notifications for flushdb/flushall events
            await _database.ExecuteAsync("CONFIG", "SET", "notify-keyspace-events", "KEA");

            // Subscribe to database flush events
            await _subscriber.SubscribeAsync("__keyevent@*__:flushdb", OnCacheCleared);
            await _subscriber.SubscribeAsync("__keyevent@*__:flushall", OnCacheCleared);
            
            // Subscribe to projection key deletion/expiration events (generic pattern)
            await _subscriber.SubscribeAsync("__keyspace@*__:*:projections:*", OnProjectionKeyEvent);

            logger.LogInformation("Cache invalidation service started, monitoring for cache events");

            // Keep the service running
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to start cache invalidation service");
            throw;
        }
    }

    private async void OnCacheCleared(RedisChannel channel, RedisValue value)
    {
        logger.LogWarning("Redis cache was cleared, rebuilding all projections...");
        await RebuildProjectionsForType(null); // null = rebuild all types
    }

    private async void OnProjectionKeyEvent(RedisChannel channel, RedisValue value)
    {
        // This fires when projection keys are deleted/expired
        if (value == "del" || value == "expired")
        {
            logger.LogWarning("Projection key deleted/expired: {Channel}", channel.ToString());
            
            // Extract the cache key prefix from the channel
            var channelStr = channel.ToString();
            var cacheKeyPrefix = ExtractCacheKeyPrefix(channelStr);
            
            if (!string.IsNullOrEmpty(cacheKeyPrefix))
            {
                // Check if we have any projections left for this type
                var hasProjections = await CheckForExistingProjections(cacheKeyPrefix);
                if (!hasProjections)
                {
                    logger.LogWarning("No projections found for {CachePrefix} after key deletion, rebuilding...", cacheKeyPrefix);
                    await RebuildProjectionsForType(cacheKeyPrefix);
                }
            }
        }
    }

    private string? ExtractCacheKeyPrefix(string channel)
    {
        // Extract cache key prefix from channel like "__keyspace@0__:example:projections:guid"
        // Should return "example"
        var parts = channel.Split(':');
        if (parts.Length >= 3 && parts[1] == "projections")
        {
            return parts[0].Split("__").LastOrDefault();
        }
        return null;
    }

    private async Task<bool> CheckForExistingProjections(string cacheKeyPrefix)
    {
        try
        {
            var server = connectionMultiplexer.GetServers().FirstOrDefault(s => !s.IsReplica);
            if (server == null)
            {
                logger.LogWarning("No non-replica Redis server found");
                return false;
            }

            var pattern = $"{cacheKeyPrefix}:projections:*";
            var keys = server.KeysAsync(pattern: pattern);
            await foreach (var key in keys)
            {
                return true; // Found at least one projection
            }
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking for existing projections with prefix {CachePrefix}", cacheKeyPrefix);
            return false;
        }
    }

    private async Task RebuildProjectionsForType(string? cacheKeyPrefix)
    {
        try
        {
            if (cacheKeyPrefix == null)
            {
                // Rebuild all projection types
                logger.LogInformation("Starting rebuild of all projections...");
                
                foreach (var cacheKey in ApplicationConstants.GetCacheKeys())
                {
                    await RebuildProjectionsForType(cacheKey);
                }
                
                logger.LogInformation("Completed rebuild of all projections");
                return;
            }

            // Rebuild specific projection type
            switch (cacheKeyPrefix)
            {
                case ApplicationConstants.ExampleCacheKey:
                    await RebuildExampleProjections();
                    break;
                // Add more cases here for other document types as needed
                // NOTE: When adding new types, also add them to ApplicationConstants.GetCacheKeys()
                default:
                    logger.LogWarning("Unknown cache key prefix for rebuild: {CachePrefix}", cacheKeyPrefix);
                    break;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error rebuilding projections for type {CachePrefix}", cacheKeyPrefix ?? "ALL");
        }
    }

    private async Task RebuildExampleProjections()
    {
        try
        {
            logger.LogInformation("Rebuilding Example projections...");
            
            var documents = await documentRepository.GetAllAsync<ExampleDocument>();
            var rebuildCount = 0;
            
            foreach (var document in documents)
            {
                var projection = mapper.Map<ExampleProjection>(document);
                await _database.StringSetAsync(ApplicationConstants.ExampleProjectionByIdCacheKey(document.Id), 
                    SerializeProjection(projection));
                rebuildCount++;
            }
            
            logger.LogInformation("Rebuilt {Count} Example projections", rebuildCount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error rebuilding Example projections");
        }
    }

    private byte[] SerializeProjection<T>(T projection) where T : class
    {
        Guard.Argument(projection).NotNull();

        using var ms = new MemoryStream();
        ProtoBuf.Serializer.Serialize(ms, projection);
        return ms.ToArray();
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Cache invalidation service stopping...");
        
        if (_subscriber != null)
        {
            await _subscriber.UnsubscribeAllAsync();
        }
        
        await base.StopAsync(cancellationToken);
    }
}