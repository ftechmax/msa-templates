using ApplicationName.Api.Application.Repositories;
using ApplicationName.Api.Contracts;
using ApplicationName.Shared.Events;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace ApplicationName.Api.Consumers;

public class LocalEventHandler(IHubContext<ApiHub> hub, IProtoCacheRepository protoCacheRepository) :
    IConsumer<IExampleCreatedEvent>
{
    public async Task Consume(ConsumeContext<IExampleCreatedEvent> context)
    {
        await protoCacheRepository.RemoveAsync(ApplicationConstants.ExampleCollectionCacheKey);

        await hub.Clients.All.SendAsync(nameof(IExampleCreatedEvent), new
        {
            context.Message.CorrelationId,
            context.Message.Id,
        });
    }
}