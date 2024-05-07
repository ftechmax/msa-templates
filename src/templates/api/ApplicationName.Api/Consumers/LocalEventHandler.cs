using ApplicationName.Api.Application.Repositories;
using ApplicationName.Api.Contracts;
using ApplicationName.Shared.Events;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace ApplicationName.Api.Consumers;

public class LocalEventHandler(IHubContext<ApiHub> hub, IProtoCacheRepository protoCacheRepository) :
    IConsumer<IExampleCreatedEvent>,
    IConsumer<IExampleUpdatedEvent>,
    IConsumer<IExampleEntityAddedEvent>,
    IConsumer<IExampleEntityUpdatedEvent>,
    IConsumer<IExampleRemoteCodeSetEvent>
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

    public async Task Consume(ConsumeContext<IExampleUpdatedEvent> context)
    {
        await protoCacheRepository.RemoveAsync(ApplicationConstants.ExampleDetailsCacheKey(context.Message.Id));
        await protoCacheRepository.RemoveAsync(ApplicationConstants.ExampleCollectionCacheKey);

        await hub.Clients.All.SendAsync(nameof(IExampleUpdatedEvent), new
        {
            context.Message.CorrelationId,
            context.Message.Id,
        });
    }

    public async Task Consume(ConsumeContext<IExampleEntityAddedEvent> context)
    {
        await protoCacheRepository.RemoveAsync(ApplicationConstants.ExampleDetailsCacheKey(context.Message.Id));

        await hub.Clients.All.SendAsync(nameof(IExampleEntityAddedEvent), new
        {
            context.Message.CorrelationId,
            context.Message.Id,
            context.Message.EntityId
        });
    }

    public async Task Consume(ConsumeContext<IExampleEntityUpdatedEvent> context)
    {
        await protoCacheRepository.RemoveAsync(ApplicationConstants.ExampleDetailsCacheKey(context.Message.Id));

        await hub.Clients.All.SendAsync(nameof(IExampleEntityUpdatedEvent), new
        {
            context.Message.CorrelationId,
            context.Message.Id,
            context.Message.EntityId
        });
    }

    public async Task Consume(ConsumeContext<IExampleRemoteCodeSetEvent> context)
    {
        await protoCacheRepository.RemoveAsync(ApplicationConstants.ExampleDetailsCacheKey(context.Message.Id));

        await hub.Clients.All.SendAsync(nameof(IExampleRemoteCodeSetEvent), new
        {
            context.Message.CorrelationId,
            context.Message.Id
        });
    }
}