using ApplicationName.Shared.Aggregates;
using MongoDB.Bson.Serialization.Attributes;

namespace ApplicationName.Api.Application.Documents;

public sealed class ExampleDocument : DocumentBase, IExample
{
    [BsonConstructor]
    private ExampleDocument()
    {
    }

    public string Name { get; init; }

    public string Description { get; init; }

    public ExampleValueObject ExampleValueObject { get; init; }

    public int? RemoteCode { get; init; }

    [BsonIgnore]
    IExampleValueObject IExample.ExampleValueObject => ExampleValueObject;
}