using System.Threading.Tasks;
using ApplicationName.Shared.Commands;
using ApplicationName.Worker.Commands;
using AutoMapper;
using MassTransit;
using Other.Worker.Contracts.Commands;

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
        var command = _mapper.Map<SetExampleRemoteCodeCommand>(context.Message);
        return context.Send<ISetExampleRemoteCodeCommand>(command);
    }
}