using ApplicationName.Api.Application.Repositories;
using Dawn;
using Microsoft.Extensions.Caching.Distributed;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

namespace ApplicationName.Api.Infrastructure;

[ExcludeFromCodeCoverage]
public sealed class ProtoCacheRepository : IProtoCacheRepository
{
    private readonly IDistributedCache _distributedCache;

    public ProtoCacheRepository(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
    }

    public async Task<T> GetAsync<T>(string key)
    {
        Guard.Argument(key, nameof(key)).NotNull().NotWhiteSpace();

        var bytes = await _distributedCache.GetAsync(key);
        if (bytes == default)
        {
            return default;
        }

        await using var ms = new MemoryStream(bytes);
        return ProtoBuf.Serializer.Deserialize<T>(ms);
    }

    public async Task SetAsync<T>(string key, T obj, DistributedCacheEntryOptions options = default) where T : class
    {
        Guard.Argument(key, nameof(key)).NotNull().NotWhiteSpace();
        Guard.Argument(obj, nameof(obj)).NotNull();

        await using var ms = new MemoryStream();

        ProtoBuf.Serializer.Serialize(ms, obj);
        ms.Position = 0;

        await _distributedCache.SetAsync(key, ms.ToArray(), options);
    }

    public Task RemoveAsync(string key)
    {
        return _distributedCache.RemoveAsync(key);
    }
}