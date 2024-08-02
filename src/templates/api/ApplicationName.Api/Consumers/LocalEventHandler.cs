using ApplicationName.Api.Application.Repositories;
using ApplicationName.Api.Contracts;
using ApplicationName.Shared.Events;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace ApplicationName.Api.Consumers;

public class LocalEventHandler(IHubContext<ApiHub> hub, IProtoCacheRepository protoCacheRepository) :
    IConsumer<ExampleCreatedEvent>,
    IConsumer<ExampleUpdatedEvent>,
    IConsumer<ExampleRemoteCodeSetEvent>
{
    public async Task Consume(ConsumeContext<ExampleCreatedEvent> context)
    {
        await protoCacheRepository.RemoveAsync(ApplicationConstants.ExampleCollectionCacheKey);

        await hub.Clients.All.SendAsync(nameof(ExampleCreatedEvent), new
        {
            context.Message.CorrelationId,
            context.Message.Id,
        });
    }

    public async Task Consume(ConsumeContext<ExampleUpdatedEvent> context)
    {
        await protoCacheRepository.RemoveAsync(ApplicationConstants.ExampleDetailsCacheKey(context.Message.Id));
        await protoCacheRepository.RemoveAsync(ApplicationConstants.ExampleCollectionCacheKey);

        await hub.Clients.All.SendAsync(nameof(ExampleUpdatedEvent), new
        {
            context.Message.CorrelationId,
            context.Message.Id,
        });
    }

    public async Task Consume(ConsumeContext<ExampleRemoteCodeSetEvent> context)
    {
        await protoCacheRepository.RemoveAsync(ApplicationConstants.ExampleDetailsCacheKey(context.Message.Id));

        await hub.Clients.All.SendAsync(nameof(ExampleRemoteCodeSetEvent), new
        {
            context.Message.CorrelationId,
            context.Message.Id
        });
    }
}