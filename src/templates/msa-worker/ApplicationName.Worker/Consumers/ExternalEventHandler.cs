using ApplicationName.Worker.Commands;
using ApplicationName.Worker.Contracts.Commands;
using AutoMapper;
using MassTransit;
using Other.Worker.Contracts.Commands;
using System.Threading.Tasks;

namespace ApplicationName.Worker.Consumers;

public class ExternalEventHandler : IConsumer<IExternalEvent>
{
    private readonly IMapper _mapper;

    public ExternalEventHandler(IMapper mapper)
    {
        _mapper = mapper;
    }

    public Task Consume(ConsumeContext<IExternalEvent> context)
    {
        var command = _mapper.Map<ExampleCommand>(context.Message);
        return context.Send<IExampleCommand>(command);
    }
}