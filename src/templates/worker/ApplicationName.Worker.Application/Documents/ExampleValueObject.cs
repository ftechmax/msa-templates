using ApplicationName.Shared.Aggregates;
using ApplicationName.Shared.Events;
using ArgDefender;
using MongoDB.Bson.Serialization.Attributes;

namespace ApplicationName.Worker.Application.Documents;

public sealed class ExampleValueObject : IExampleValueObject
{
    [BsonConstructor]
    private ExampleValueObject()
    {
    }

    public ExampleValueObject(ExampleValueObjectEventData eventData)
        : this()
    {
        Guard.Argument(eventData, nameof(eventData)).NotNull();
        Guard.Argument(eventData.Code, nameof(eventData.Code)).NotNull().NotWhiteSpace();
        Guard.Argument(eventData.Value, nameof(eventData.Value)).NotNegative();

        Code = eventData.Code;
        Value = eventData.Value;
    }

    public string Code { get; init; }

    public double Value { get; init; }
}