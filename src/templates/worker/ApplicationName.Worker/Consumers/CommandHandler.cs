using ApplicationName.Shared.Commands;
using ApplicationName.Shared.Events;
using ApplicationName.Worker.Application.Services;
using ApplicationName.Worker.Contracts.Commands;
using ApplicationName.Worker.Events;
using AutoMapper;
using MassTransit;

namespace ApplicationName.Worker.Consumers;

public class CommandHandler(IApplicationService applicationService, IMapper mapper) :
    IConsumer<ICreateExampleCommand>,
    IConsumer<IUpdateExampleCommand>,
    IConsumer<IAddExampleEntityCommand>,
    IConsumer<IUpdateExampleEntityCommand>,
    IConsumer<ISetExampleRemoteCodeCommand>
{
    public async Task Consume(ConsumeContext<ICreateExampleCommand> context)
    {
        var domainEvent = await applicationService.HandleAsync(context.Message);
        if (domainEvent != default)
        {
            var @event = mapper.Map<ExampleCreatedEvent>(domainEvent);
            @event.CorrelationId = context.Message.CorrelationId;
            await context.Publish<IExampleCreatedEvent>(@event);
        }
    }

    public async Task Consume(ConsumeContext<IUpdateExampleCommand> context)
    {
        var domainEvent = await applicationService.HandleAsync(context.Message);
        if (domainEvent != default)
        {
            var @event = mapper.Map<ExampleUpdatedEvent>(domainEvent);
            @event.CorrelationId = context.Message.CorrelationId;
            await context.Publish<IExampleUpdatedEvent>(@event);
        }
    }

    public async Task Consume(ConsumeContext<IAddExampleEntityCommand> context)
    {
        var domainEvent = await applicationService.HandleAsync(context.Message);
        if (domainEvent != default)
        {
            var @event = mapper.Map<ExampleEntityAddedEvent>(domainEvent);
            @event.CorrelationId = context.Message.CorrelationId;
            await context.Publish<IExampleEntityAddedEvent>(@event);
        }
    }

    public async Task Consume(ConsumeContext<IUpdateExampleEntityCommand> context)
    {
        var domainEvent = await applicationService.HandleAsync(context.Message);
        if (domainEvent != default)
        {
            var @event = mapper.Map<ExampleEntityUpdatedEvent>(domainEvent);
            @event.CorrelationId = context.Message.CorrelationId;
            await context.Publish<IExampleEntityUpdatedEvent>(@event);
        }
    }

    public async Task Consume(ConsumeContext<ISetExampleRemoteCodeCommand> context)
    {
        var domainEvent = await applicationService.HandleAsync(context.Message);
        if (domainEvent != default)
        {
            var @event = mapper.Map<ExampleRemoteCodeSetEvent>(domainEvent);
            @event.CorrelationId = context.Message.CorrelationId;
            await context.Publish<IExampleRemoteCodeSetEvent>(@event);
        }
    }
}