using System.Diagnostics.CodeAnalysis;
using ArgDefender;
using StackExchange.Redis;

namespace ApplicationName.Worker.Infrastructure;

[ExcludeFromCodeCoverage]
public sealed class ProtoCacheRepository(IConnectionMultiplexer connectionMultiplexer) : IProtoCacheRepository
{
    private IDatabase Database => connectionMultiplexer.GetDatabase();

    public async Task<T?> GetAsync<T>(string key)
    {
        Guard.Argument(key).NotNull().NotWhiteSpace();

        var bytes = await Database.StringGetAsync(key);
        if (bytes == default || bytes.IsNull)
        {
            return default;
        }

        await using var ms = new MemoryStream(bytes!);
        return ProtoBuf.Serializer.Deserialize<T>(ms);
    }

    public async Task SetAsync<T>(string key, T obj, TimeSpan? expiry = null) where T : class
    {
        Guard.Argument(key).NotNull().NotWhiteSpace();
        Guard.Argument(obj).NotNull();

        await using var ms = new MemoryStream();

        ProtoBuf.Serializer.Serialize(ms, obj);
        ms.Position = 0;

        await Database.StringSetAsync(key, ms.ToArray(), expiry, When.Always);
    }

    public async Task SetAsync(string key, object obj, TimeSpan? expiry = null)
    {
        Guard.Argument(key).NotNull().NotWhiteSpace();
        Guard.Argument(obj).NotNull();

        await using var ms = new MemoryStream();

        ProtoBuf.Serializer.NonGeneric.Serialize(ms, obj);
        ms.Position = 0;

        await Database.StringSetAsync(key, ms.ToArray(), expiry, When.Always);
    }

    public Task RemoveAsync(string key)
    {
        return Database.KeyDeleteAsync(new RedisKey(key));
    }
}
