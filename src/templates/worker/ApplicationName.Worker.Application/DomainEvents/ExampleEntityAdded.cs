using ApplicationName.Shared.Aggregates;

namespace ApplicationName.Worker.Application.DomainEvents;

public record ExampleEntityAdded : DomainEvent
{
    public ExampleEntityAdded(Guid aggregateId, IExampleEntity entity)
    {
        Id = aggregateId;
        Timestamp = DateTime.UtcNow;
        ExampleId = entity.Id;
        Name = entity.Name;
        SomeValue = entity.SomeValue;
    }

    public Guid ExampleId { get; init; }

    public string Name { get; init; }

    public float SomeValue { get; init; }
}