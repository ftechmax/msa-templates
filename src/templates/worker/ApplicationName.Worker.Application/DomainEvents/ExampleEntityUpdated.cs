using ApplicationName.Shared.Aggregates;

namespace ApplicationName.Worker.Application.DomainEvents;

public record ExampleEntityUpdated : DomainEvent
{
    public ExampleEntityUpdated(Guid aggregateId, IExampleEntity entity)
    {
        Id = aggregateId;
        EntityId = entity.Id;
        Timestamp = DateTime.UtcNow;
        SomeValue = entity.SomeValue;
    }

    public Guid EntityId { get; init; }

    public float SomeValue { get; init; }
}