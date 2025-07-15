using System.Diagnostics.CodeAnalysis;
using ApplicationName.Api.Application.Repositories;
using ArgDefender;
using Microsoft.Extensions.Caching.Distributed;

namespace ApplicationName.Api.Infrastructure;

[ExcludeFromCodeCoverage]
public sealed class ProtoCacheRepository(IDistributedCache distributedCache) : IProtoCacheRepository
{
    public async Task<T?> GetAsync<T>(string key)
    {
        Guard.Argument(key).NotNull().NotWhiteSpace();

        var bytes = await distributedCache.GetAsync(key);
        if (bytes == null)
        {
            return default;
        }

        await using var ms = new MemoryStream(bytes);
        return ProtoBuf.Serializer.Deserialize<T>(ms);
    }

    public async Task SetAsync<T>(string key, T obj, DistributedCacheEntryOptions? options = null) where T : class
    {
        Guard.Argument(key).NotNull().NotWhiteSpace();
        Guard.Argument(obj).NotNull();

        await using var ms = new MemoryStream();

        ProtoBuf.Serializer.Serialize(ms, obj);
        ms.Position = 0;

        await distributedCache.SetAsync(key, ms.ToArray(), options ?? new DistributedCacheEntryOptions());
    }

    public Task RemoveAsync(string key)
    {
        return distributedCache.RemoveAsync(key);
    }
}