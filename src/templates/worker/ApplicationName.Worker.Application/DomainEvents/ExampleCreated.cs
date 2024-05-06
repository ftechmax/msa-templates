using ApplicationName.Shared.Aggregates;

namespace ApplicationName.Worker.Application.DomainEvents;

public record ExampleCreated : DomainEvent
{
    public ExampleCreated(IExample aggregate)
    {
        Id = aggregate.Id;
        Timestamp = DateTime.UtcNow;
        Name = aggregate.Name;
        Description = aggregate.Description;
        ExampleValueObject = new ExampleValueObjectEvent(aggregate.ExampleValueObject);
    }

    public string Name { get; init; }

    public string Description { get; init; }

    public ExampleValueObjectEvent ExampleValueObject { get; init; }
}