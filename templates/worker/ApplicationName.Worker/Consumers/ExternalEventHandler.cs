using ApplicationName.Worker.Contracts.Commands;
using MapsterMapper;
using MassTransit;
using Other.Worker.Contracts.Commands;

namespace ApplicationName.Worker.Consumers;

public class ExternalEventHandler(IMapper mapper) : IConsumer<ExternalEvent>
{
    public Task Consume(ConsumeContext<ExternalEvent> context)
    {
        var command = mapper.Map<SetExampleRemoteCodeCommand>(context.Message);
        return context.Send(command);
    }
}