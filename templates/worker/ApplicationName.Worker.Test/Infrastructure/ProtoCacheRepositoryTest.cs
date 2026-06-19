using ApplicationName.Worker.Infrastructure;
using FakeItEasy;
using NUnit.Framework;
using ProtoBuf;
using Shouldly;
using StackExchange.Redis;

namespace ApplicationName.Worker.Test.Infrastructure;

public class ProtoCacheRepositoryTest
{
    private IConnectionMultiplexer _connectionMultiplexer;

    private IDatabase _database;

    private ProtoCacheRepository _subjectUnderTest;

    [SetUp]
    public void Setup()
    {
        _connectionMultiplexer = A.Fake<IConnectionMultiplexer>();
        _database = A.Fake<IDatabase>();

        A.CallTo(() => _connectionMultiplexer.GetDatabase(A<int>._, A<object>._)).Returns(_database);

        _subjectUnderTest = new ProtoCacheRepository(_connectionMultiplexer);
    }

    [Test]
    public async Task GetAsync_WhenStringValueExists_DeserializesValue()
    {
        // Arrange
        var data = new TestProjection { Id = Guid.NewGuid(), Name = "name" };

        A.CallTo(() => _database.StringGetAsync("key", A<CommandFlags>._))
            .Returns(Serialize(data));

        // Act
        var result = await _subjectUnderTest.GetAsync<TestProjection>("key");

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(data.Id);
        result.Name.ShouldBe(data.Name);
    }

    [Test]
    public async Task SetAsync_WritesValueAsRedisString()
    {
        // Arrange
        var data = new TestProjection { Id = Guid.NewGuid(), Name = "name" };

        // Act
        await _subjectUnderTest.SetAsync("key", data);

        // Assert
        var call = Fake.GetCalls(_database)
            .Single(i => i.Method.Name == nameof(IDatabase.StringSetAsync));

        call.Arguments[0].ShouldBe((RedisKey)"key");
        call.Arguments[1].ShouldBeOfType<RedisValue>();
        call.Arguments[2].ShouldBeNull();
        call.Arguments[3].ShouldBe(When.Always);
    }

    private static byte[] Serialize<T>(T data)
    {
        using var ms = new MemoryStream();
        Serializer.Serialize(ms, data);
        return ms.ToArray();
    }

    [ProtoContract]
    private sealed class TestProjection
    {
        [ProtoMember(1)]
        public Guid Id { get; set; }

        [ProtoMember(2)]
        public string Name { get; set; } = "";
    }
}
