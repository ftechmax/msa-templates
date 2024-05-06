using ApplicationName.Shared.Aggregates;

namespace ApplicationName.Worker.Application.DomainEvents;

public record ExampleEntityUpdated : DomainEvent
{
    public ExampleEntityUpdated(Guid aggregateId, IExampleEntity entity)
    {
        Id = aggregateId;
        Timestamp = DateTime.UtcNow;
        SomeValue = entity.SomeValue;
    }

    public float SomeValue { get; init; }
}