using ApplicationName.Shared.Aggregates;

namespace ApplicationName.Worker.Application.DomainEvents;

public record ExampleUpdated : DomainEvent
{
    public ExampleUpdated(IExample aggregate)
    {
        Id = aggregate.Id;
        Timestamp = DateTime.UtcNow;
        Description = aggregate.Description;
        ExampleValueObject = new ExampleValueObjectEvent(aggregate.ExampleValueObject);
    }

    public string Description { get; init; }

    public ExampleValueObjectEvent ExampleValueObject { get; init; }
}