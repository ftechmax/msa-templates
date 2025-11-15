using Microsoft.Extensions.Caching.Distributed;

namespace ApplicationName.Worker.Infrastructure;

public interface IProtoCacheRepository
{
    Task<T?> GetAsync<T>(string key);

    Task SetAsync<T>(string key, T obj, DistributedCacheEntryOptions options = default) where T : class;

    Task RemoveAsync(string key);
}