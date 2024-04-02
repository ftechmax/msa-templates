using ApplicationName.Worker.Application.Documents;

namespace ApplicationName.Worker.Application.DomainEvents;

public class ExampleCreated : DomainEvent
{
    public ExampleCreated(ExampleDocument aggregate)
    {
        Id = aggregate.Id;
        Timestamp = DateTime.UtcNow;
        Name = aggregate.Name;
        Description = aggregate.Description;
        ExampleValueObjectCode = aggregate.ExampleValueObject.Code;
        ExampleValueObjectValue = aggregate.ExampleValueObject.Value;
    }

    public string Name { get; }

    public string Description { get; }

    public string ExampleValueObjectCode { get; }

    public double ExampleValueObjectValue { get; }
}