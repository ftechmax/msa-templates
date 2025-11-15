using System.Diagnostics.CodeAnalysis;
using ApplicationName.Api.Application.Repositories;
using ArgDefender;
using StackExchange.Redis;

namespace ApplicationName.Api.Infrastructure;

[ExcludeFromCodeCoverage]
public sealed class ProtoCacheRepository(IConnectionMultiplexer connectionMultiplexer) : IProtoCacheRepository
{
    private IDatabase Database => connectionMultiplexer.GetDatabase();

    private IServer[] Servers => connectionMultiplexer.GetServers();

    public async Task<T> GetAsync<T>(string key)
    {
        Guard.Argument(key).NotNull().NotWhiteSpace();

        var bytes = await Database.StringGetAsync(key);
        if (bytes == default)
        {
            return default;
        }

        await using var ms = new MemoryStream(bytes);
        return ProtoBuf.Serializer.Deserialize<T>(ms);
    }

    public async Task<IEnumerable<T>> GetAllAsync<T>(string keyNamespace)
    {
        Guard.Argument(keyNamespace).NotNull().NotWhiteSpace();

        var server = Servers.First(i => i.IsReplica == false);

        var keysAsync = server.KeysAsync(pattern: new RedisValue($"{keyNamespace}:*"));
        var keys = new List<RedisKey>();
        await foreach (var key in keysAsync)
        {
            keys.Add(key);
        }

        var results = new List<T>();
        if (keys.Count == 0)
        {
            return results;
        }

        var resultSet = await Database.StringGetAsync([.. keys]);
        foreach (var value in resultSet)
        {
            await using var ms = new MemoryStream(value);
            results.Add(ProtoBuf.Serializer.Deserialize<T>(ms));
        }

        return results;
    }

    public async Task SetAsync<T>(string key, T obj, TimeSpan? expiry = null) where T : class
    {
        Guard.Argument(key).NotNull().NotWhiteSpace();
        Guard.Argument(obj).NotNull();

        await using var ms = new MemoryStream();

        ProtoBuf.Serializer.Serialize(ms, obj);
        ms.Position = 0;

        await Database.StringSetAsync(key, ms.ToArray(), expiry);
    }

    public Task RemoveAsync(string key)
    {
        return Database.KeyDeleteAsync(new RedisKey(key));
    }
}