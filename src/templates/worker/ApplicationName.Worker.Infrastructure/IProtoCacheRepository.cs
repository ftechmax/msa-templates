using Microsoft.Extensions.Caching.Distributed;

namespace ApplicationName.Worker.Infrastructure;

public interface IProtoCacheRepository
{
    Task<T?> GetAsync<T>(string key);

    Task SetAsync<T>(string key, T obj, DistributedCacheEntryOptions? options = null) where T : class;

    Task SetAsync(string key, object obj, DistributedCacheEntryOptions? options = null);

    Task RemoveAsync(string key);
}
