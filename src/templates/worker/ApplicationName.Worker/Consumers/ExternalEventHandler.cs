using System.Threading.Tasks;
using ApplicationName.Worker.Commands;
using ApplicationName.Worker.Contracts.Commands;
using AutoMapper;
using MassTransit;
using Other.Worker.Contracts.Commands;

namespace ApplicationName.Worker.Consumers;

public class ExternalEventHandler(IMapper mapper) : IConsumer<IExternalEvent>
{
    public Task Consume(ConsumeContext<IExternalEvent> context)
    {
        var command = mapper.Map<SetExampleRemoteCodeCommand>(context.Message);
        return context.Send<ISetExampleRemoteCodeCommand>(command);
    }
}