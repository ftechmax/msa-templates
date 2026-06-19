namespace ApplicationName.Worker.Infrastructure;

public interface IProtoCacheRepository
{
    Task<T?> GetAsync<T>(string key);

    Task SetAsync<T>(string key, T obj, TimeSpan? expiry = null) where T : class;

    Task SetAsync(string key, object obj, TimeSpan? expiry = null);

    Task RemoveAsync(string key);
}
