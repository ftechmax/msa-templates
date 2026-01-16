using ApplicationName.Api.Contracts;
using ApplicationName.Api.Infrastructure;
using ApplicationName.Shared.Projections;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using NUnit.Framework;
using Shouldly;
using StackExchange.Redis;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ApplicationName.Api.Test.Infrastructure;

public class ProtoCacheRepositoryTest
{
    private IFixture _fixture;

    private IConnectionMultiplexer _connectionMultiplexer;

    private IDatabase _database;

    private IServer _server;

    private ProtoCacheRepository _subjectUnderTest;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization());

        _connectionMultiplexer = _fixture.Freeze<IConnectionMultiplexer>();
        _database = _fixture.Freeze<IDatabase>();
        _server = _fixture.Freeze<IServer>();

        A.CallTo(() => _connectionMultiplexer.GetDatabase(A<int>._, A<object>._)).Returns(_database);
        A.CallTo(() => _connectionMultiplexer.GetServers()).Returns(new[] { _server });
        A.CallTo(() => _server.IsReplica).Returns(false);

        _subjectUnderTest = new ProtoCacheRepository(_connectionMultiplexer);
    }

    [Test]
    public async Task GetAllAsync_WhenSomeValuesMissing_SkipsNullValues()
    {
        // Arrange
        var projection = new ExampleProjection
        {
            Id = Guid.NewGuid(),
            Created = DateTime.UtcNow,
            Updated = DateTime.UtcNow,
            Name = "name",
            Description = "description",
            ExampleValueObject = new ExampleValueObjectProjection { Code = "code", Value = 1.23 }
        };

        var key1 = (RedisKey)$"projections:example:{projection.Id:N}";
        var key2 = (RedisKey)$"projections:example:{Guid.NewGuid():N}";

        A.CallTo(() => _server.KeysAsync(A<int>._, A<RedisValue>._, A<int>._, A<long>._, A<int>._, A<CommandFlags>._))
            .ReturnsLazily((int _, RedisValue _, int _, long _, int _, CommandFlags _) => GetKeys(key1, key2));

        A.CallTo(() => _database.StringGetAsync(A<RedisKey[]>._, A<CommandFlags>._))
            .ReturnsLazily((RedisKey[] _, CommandFlags _) => Task.FromResult(new[]
            {
                (RedisValue)Serialize(projection),
                RedisValue.Null
            }));

        // Act
        var result = await _subjectUnderTest.GetAllAsync<ExampleProjection>(ApplicationConstants.ExampleProjectionCacheKey);

        // Assert
        result.Count().ShouldBe(1);

        var item = result.Single();
        item.Name.ShouldBe(projection.Name);
        item.Description.ShouldBe(projection.Description);
        item.ExampleValueObject.Code.ShouldBe(projection.ExampleValueObject.Code);
        item.ExampleValueObject.Value.ShouldBe(projection.ExampleValueObject.Value);
    }

    private static byte[] Serialize<T>(T data)
    {
        using var ms = new MemoryStream();
        ProtoBuf.Serializer.Serialize(ms, data);
        return ms.ToArray();
    }

    private static async IAsyncEnumerable<RedisKey> GetKeys(params RedisKey[] keys)
    {
        foreach (var key in keys)
        {
            yield return key;
            await Task.Yield();
        }
    }
}
