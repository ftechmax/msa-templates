using System;
using ApplicationName.Worker.Application.Documents;

namespace ApplicationName.Worker.Application.DomainEvents;

public class ExampleUpdated : DomainEvent
{
    public ExampleUpdated(ExampleDocument aggregate)
    {
        Id = aggregate.Id;
        Timestamp = DateTime.UtcNow;
        Description = aggregate.Description;
        ExampleValueObjectCode = aggregate.ExampleValueObject.Code;
        ExampleValueObjectValue = aggregate.ExampleValueObject.Value;
    }

    public string Description { get; }

    public string ExampleValueObjectCode { get; }

    public double ExampleValueObjectValue { get; }
}