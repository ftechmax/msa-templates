using ApplicationName.Shared.Aggregates;
using MongoDB.Bson.Serialization.Attributes;

namespace ApplicationName.Api.Application.Documents;

public sealed class ExampleValueObject : IExampleValueObject
{
    [BsonConstructor]
    private ExampleValueObject()
    {
    }

    public string Code { get; init; }

    public double Value { get; init; }
}