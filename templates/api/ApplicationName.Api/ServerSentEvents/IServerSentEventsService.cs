using System.Net.ServerSentEvents;

namespace ApplicationName.Api.ServerSentEvents;

public interface IServerSentEventsService
{
    ValueTask PublishAsync(string eventType, object data, CancellationToken cancellationToken = default);

    IAsyncEnumerable<SseItem<object>> SubscribeAsync(CancellationToken cancellationToken);
}
