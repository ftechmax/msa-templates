using ApplicationName.Shared.Events;

namespace ApplicationName.Worker.Events;

public class ExampleEntityUpdatedEvent : IExampleEntityUpdatedEvent
{
    public Guid CorrelationId { get; set; }

    public Guid Id { get; set; }

    public Guid EntityId { get; set; }
}