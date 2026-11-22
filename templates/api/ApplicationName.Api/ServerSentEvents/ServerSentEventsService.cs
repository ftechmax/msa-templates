using System.Collections.Concurrent;
using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Channels;
using ApplicationName.Api.Contracts;
using StackExchange.Redis;

namespace ApplicationName.Api.ServerSentEvents;

public sealed class ServerSentEventsService(
    IConnectionMultiplexer connectionMultiplexer,
    ILogger<ServerSentEventsService> logger) : IServerSentEventsService, IHostedService
{
    private static readonly RedisChannel EventsChannel = RedisChannel.Literal($"{ApplicationConstants.ApplicationKey}:sse");

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly ConcurrentDictionary<Guid, Channel<SseItem<object>>> _clients = new();

    // Fire-and-forget registers the handler even while Valkey is down; the multiplexer subscribes on connect.
    public Task StartAsync(CancellationToken cancellationToken)
    {
        return connectionMultiplexer.GetSubscriber().SubscribeAsync(EventsChannel, OnMessage, CommandFlags.FireAndForget);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return connectionMultiplexer.GetSubscriber().UnsubscribeAsync(EventsChannel, OnMessage, CommandFlags.FireAndForget);
    }

    public async ValueTask PublishAsync(string eventType, object data, CancellationToken cancellationToken = default)
    {
        var message = new ServerSentEventMessage(eventType, JsonSerializer.SerializeToElement(data, data.GetType(), JsonOptions));

        await connectionMultiplexer.GetSubscriber()
            .PublishAsync(EventsChannel, JsonSerializer.Serialize(message, JsonOptions))
            .WaitAsync(cancellationToken);
    }

    public async IAsyncEnumerable<SseItem<object>> SubscribeAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var channel = Channel.CreateUnbounded<SseItem<object>>(new UnboundedChannelOptions
        {
            SingleReader = true,
        });
        var clientId = Guid.NewGuid();
        _clients[clientId] = channel;

        try
        {
            await foreach (var item in channel.Reader.ReadAllAsync(cancellationToken))
            {
                yield return item;
            }
        }
        finally
        {
            _clients.TryRemove(clientId, out _);
        }
    }

    private void OnMessage(RedisChannel channel, RedisValue value)
    {
        try
        {
            var message = JsonSerializer.Deserialize<ServerSentEventMessage>(value.ToString(), JsonOptions)!;
            var item = new SseItem<object>(message.Data, message.EventType);

            foreach (var client in _clients.Values)
            {
                client.Writer.TryWrite(item);
            }
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Ignoring malformed server-sent event message");
        }
    }

    private sealed record ServerSentEventMessage(string EventType, JsonElement Data);
}
