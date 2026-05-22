namespace ApplicationName.Api.Application.Repositories;

public interface IProtoCacheRepository
{
    Task<T?> GetAsync<T>(string key);

    Task<IEnumerable<T>> GetAllAsync<T>(string keyNamespace);

    Task SetAsync<T>(string key, T obj, TimeSpan? expiry = null) where T : class;

    Task RemoveAsync(string key);
}