using ApplicationName.Worker.Application.Documents;

namespace ApplicationName.Worker.Application.DomainEvents;

public class ExampleEntityUpdated : DomainEvent
{
    public ExampleEntityUpdated(Guid aggregateId, ExampleEntity entity)
    {
        Id = aggregateId;
        Timestamp = DateTime.UtcNow;
    }
}