using ApplicationName.Shared.Commands;
using ApplicationName.Shared.Events;
using ApplicationName.Worker.Application.Services;
using ApplicationName.Worker.Contracts.Commands;
using AutoMapper;
using MassTransit;

namespace ApplicationName.Worker.Consumers;

public class CommandHandler(IApplicationService applicationService, IMapper mapper) :
    IConsumer<CreateExampleCommand>,
    IConsumer<UpdateExampleCommand>,
    IConsumer<SetExampleRemoteCodeCommand>
{
    public async Task Consume(ConsumeContext<CreateExampleCommand> context)
    {
        var domainEvent = await applicationService.HandleAsync(context.Message);
        if (domainEvent != default)
        {
            var @event = mapper.Map<ExampleCreatedEvent>(domainEvent) with { CorrelationId = context.Message.CorrelationId };
            await context.Publish(@event);
        }
    }

    public async Task Consume(ConsumeContext<UpdateExampleCommand> context)
    {
        var domainEvent = await applicationService.HandleAsync(context.Message);
        if (domainEvent != default)
        {
            var @event = mapper.Map<ExampleUpdatedEvent>(domainEvent) with { CorrelationId = context.Message.CorrelationId };
            await context.Publish(@event);
        }
    }

    public async Task Consume(ConsumeContext<SetExampleRemoteCodeCommand> context)
    {
        var domainEvent = await applicationService.HandleAsync(context.Message);
        if (domainEvent != default)
        {
            var @event = mapper.Map<ExampleRemoteCodeSetEvent>(domainEvent) with { CorrelationId = context.Message.CorrelationId };
            await context.Publish(@event);
        }
    }
}