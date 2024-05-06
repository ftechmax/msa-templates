using ApplicationName.Shared.Aggregates;

namespace ApplicationName.Worker.Application.DomainEvents;

public record ExampleUpdated : DomainEvent
{
    public ExampleUpdated(IExample aggregate)
    {
        Id = aggregate.Id;
        Timestamp = DateTime.UtcNow;
        Description = aggregate.Description;
        ExampleValueObjectCode = aggregate.ExampleValueObject.Code;
        ExampleValueObjectValue = aggregate.ExampleValueObject.Value;
    }

    public string Description { get; init; }

    public string ExampleValueObjectCode { get; init; }

    public double ExampleValueObjectValue { get; init; }
}