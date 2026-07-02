using System.Collections.Concurrent;
using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace ApplicationName.Api.ServerSentEvents;

public sealed class ServerSentEventsService : IServerSentEventsService
{
    private readonly ConcurrentDictionary<Guid, Channel<SseItem<object>>> _clients = new();

    public async ValueTask PublishAsync(string eventType, object data, CancellationToken cancellationToken = default)
    {
        var item = new SseItem<object>(data, eventType);

        foreach (var client in _clients.Values)
        {
            await client.Writer.WriteAsync(item, cancellationToken);
        }
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
}
