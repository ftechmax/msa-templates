using ApplicationName.Shared.Aggregates;
using ApplicationName.Shared.Events;
using ArgDefender;
using MongoDB.Bson.Serialization.Attributes;

namespace ApplicationName.Worker.Application.Documents;

public sealed class ExampleValueObject : IExampleValueObject
{
    // Suppress CS8618: Non-nullable property must contain a non-null value when exiting constructor.
    // This constructor is used by the database driver for deserialization and properties are set via reflection.
#pragma warning disable CS8618
    [BsonConstructor]
    private ExampleValueObject()
    {
    }
#pragma warning restore CS8618

    public ExampleValueObject(ExampleValueObjectEventData eventData)
        : this()
    {
        Guard.Argument(eventData, nameof(eventData)).NotNull();
        Guard.Argument(eventData.Code, nameof(eventData.Code)).NotNull().NotWhiteSpace();
        Guard.Argument(eventData.Value, nameof(eventData.Value)).NotNegative();

        Code = eventData.Code;
        Value = eventData.Value;
    }

    public string Code { get; }

    public double Value { get; }
}