using ApplicationName.Shared.Aggregates;

namespace ApplicationName.Worker.Application.DomainEvents;

public record ExampleValueObjectEvent
{
    public ExampleValueObjectEvent(IExampleValueObject valueObject)
    {
        Code = valueObject.Code;
        Value = valueObject.Value;
    }

    public string Code { get; init; }

    public double Value { get; init; }
}