using System.Text.Json;
using ApplicationName.Api.ServerSentEvents;
using FakeItEasy;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using StackExchange.Redis;

namespace ApplicationName.Api.Test.ServerSentEvents;

public class ServerSentEventsServiceTest
{
    private ISubscriber _subscriber;
    private IConnectionMultiplexer _multiplexer;
    private Action<RedisChannel, RedisValue> _handler;

    private ServerSentEventsService _subjectUnderTest;

    [SetUp]
    public void SetUp()
    {
        _subscriber = A.Fake<ISubscriber>();
        _multiplexer = A.Fake<IConnectionMultiplexer>();
        A.CallTo(() => _multiplexer.GetSubscriber(A<object>._)).Returns(_subscriber);
        A.CallTo(() => _subscriber.SubscribeAsync(A<RedisChannel>._, A<Action<RedisChannel, RedisValue>>._, A<CommandFlags>._))
            .Invokes((RedisChannel _, Action<RedisChannel, RedisValue> handler, CommandFlags _) => _handler = handler)
            .Returns(Task.CompletedTask);

        _subjectUnderTest = new ServerSentEventsService(_multiplexer, NullLogger<ServerSentEventsService>.Instance);
    }

    [TearDown]
    public void TearDown()
    {
        _multiplexer.Dispose();
    }

    [Test]
    public async Task PublishAsync_PublishesToValkeyInsteadOfLocalClients()
    {
        // Arrange
        RedisValue published = default;
        A.CallTo(() => _subscriber.PublishAsync(A<RedisChannel>._, A<RedisValue>._, A<CommandFlags>._))
            .Invokes((RedisChannel _, RedisValue value, CommandFlags _) => published = value)
            .Returns(Task.FromResult(1L));

        // Act
        await _subjectUnderTest.PublishAsync("ExampleCreatedEvent", new { Id = 42 });

        // Assert
        using var document = JsonDocument.Parse(published.ToString());
        Assert.That(document.RootElement.GetProperty("eventType").GetString(), Is.EqualTo("ExampleCreatedEvent"));
        Assert.That(document.RootElement.GetProperty("data").GetProperty("id").GetInt32(), Is.EqualTo(42));
    }

    [Test]
    public async Task ValkeyMessage_ReachesEveryLocalClient()
    {
        // Arrange
        await _subjectUnderTest.StartAsync(CancellationToken.None);
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await using var first = _subjectUnderTest.SubscribeAsync(cancellation.Token).GetAsyncEnumerator(cancellation.Token);
        await using var second = _subjectUnderTest.SubscribeAsync(cancellation.Token).GetAsyncEnumerator(cancellation.Token);
        var firstRead = first.MoveNextAsync();
        var secondRead = second.MoveNextAsync();

        // Act
        _handler(RedisChannel.Literal("test"), """{"eventType":"ExampleUpdatedEvent","data":{"id":7}}""");

        // Assert
        Assert.That(await firstRead, Is.True);
        Assert.That(await secondRead, Is.True);
        Assert.That(first.Current.EventType, Is.EqualTo("ExampleUpdatedEvent"));
        Assert.That(second.Current.EventType, Is.EqualTo("ExampleUpdatedEvent"));
        Assert.That(((JsonElement)first.Current.Data).GetProperty("id").GetInt32(), Is.EqualTo(7));
    }
}
