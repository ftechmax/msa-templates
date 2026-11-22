using System.Diagnostics;
using ApplicationName.Api.Application.Repositories;
using ApplicationName.Api.Contracts;
using ApplicationName.Api.ServerSentEvents;
using ApplicationName.Shared.Commands;
using ApplicationName.Shared.Events;
using Conveyo;

namespace ApplicationName.Api.Consumers;

public class LocalEventHandler(IServerSentEventsService events, IProtoCacheRepository protoCacheRepository) :
    IConsumer<ExampleCreatedEvent>,
    IConsumer<ExampleUpdatedEvent>,
    IConsumer<ExampleRemoteCodeSetEvent>,
    IConsumer<Fault<CreateExampleCommand>>,
    IConsumer<Fault<UpdateExampleCommand>>
{
    public async Task Consume(ConsumeContext<ExampleCreatedEvent> context)
    {
        await protoCacheRepository.RemoveAsync(ApplicationConstants.ExampleCollectionCacheKey);

        await events.PublishAsync(nameof(ExampleCreatedEvent), new
        {
            context.Message.CorrelationId,
            context.Message.Id,
        });
    }

    public async Task Consume(ConsumeContext<ExampleUpdatedEvent> context)
    {
        await protoCacheRepository.RemoveAsync(ApplicationConstants.ExampleDetailsCacheKey(context.Message.Id));
        await protoCacheRepository.RemoveAsync(ApplicationConstants.ExampleCollectionCacheKey);

        await events.PublishAsync(nameof(ExampleUpdatedEvent), new
        {
            context.Message.CorrelationId,
            context.Message.Id,
        });
    }

    public async Task Consume(ConsumeContext<ExampleRemoteCodeSetEvent> context)
    {
        await protoCacheRepository.RemoveAsync(ApplicationConstants.ExampleDetailsCacheKey(context.Message.Id));

        await events.PublishAsync(nameof(ExampleRemoteCodeSetEvent), new
        {
            context.Message.CorrelationId,
            context.Message.Id
        });
    }

    public Task Consume(ConsumeContext<Fault<CreateExampleCommand>> context)
    {
        return PublishFaultAsync<CreateExampleCommand>(context.Message.Message.CorrelationId, context.Message);
    }

    public Task Consume(ConsumeContext<Fault<UpdateExampleCommand>> context)
    {
        return PublishFaultAsync<UpdateExampleCommand>(context.Message.Message.CorrelationId, context.Message);
    }

    private async Task PublishFaultAsync<TCommand>(Guid correlationId, Fault<TCommand> fault) where TCommand : class
    {
        await events.PublishAsync($"{nameof(DomainFault)}_{typeof(TCommand).Name}",
            new DomainFault(
                correlationId,
                fault.Exceptions[0].Message,
                Activity.Current?.TraceId.ToString() ?? string.Empty
            ));
    }
}
