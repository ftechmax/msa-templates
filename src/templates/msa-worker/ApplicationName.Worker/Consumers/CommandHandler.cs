using ApplicationName.Worker.Application.Services;
using ApplicationName.Worker.Contracts.Commands;
using ApplicationName.Worker.Contracts.Events;
using ApplicationName.Worker.Events;
using AutoMapper;
using MassTransit;
using System.Threading.Tasks;

namespace ApplicationName.Worker.Consumers;

public class CommandHandler : IConsumer<IExampleCommand>
{
    private readonly IApplicationService _applicationService;

    private readonly IMapper _mapper;

    public CommandHandler(IApplicationService applicationService, IMapper mapper)
    {
        _applicationService = applicationService;
        _mapper = mapper;
    }

    public async Task Consume(ConsumeContext<IExampleCommand> context)
    {
        var document = await _applicationService.HandleAsync(context.Message);

        if (document.Created == document.Updated)
        {
            var @event = _mapper.Map<ExampleCreatedEvent>(document);
            await context.Publish<IExampleCreatedEvent>(@event);
        }
        else
        {
            var @event = _mapper.Map<ExampleUpdatedEvent>(document);
            await context.Publish<IExampleUpdatedEvent>(@event);
        }
    }
}