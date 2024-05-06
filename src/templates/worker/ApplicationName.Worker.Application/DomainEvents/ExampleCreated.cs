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
        ExampleValueObjectCode = aggregate.ExampleValueObject.Code;
        ExampleValueObjectValue = aggregate.ExampleValueObject.Value;
    }

    public string Name { get; init; }

    public string Description { get; init; }

    public string ExampleValueObjectCode { get; init; }

    public double ExampleValueObjectValue { get; init; }
}