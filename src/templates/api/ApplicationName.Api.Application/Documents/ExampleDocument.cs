using ApplicationName.Shared.Aggregates;
using MongoDB.Bson.Serialization.Attributes;

namespace ApplicationName.Api.Application.Documents;

public sealed class ExampleDocument : DocumentBase, IExample
{
    //[BsonConstructor]
    //private ExampleDocument()
    //{
    //}

    public required string Name { get; init; }

    public required string Description { get; init; }

    public required ExampleValueObject ExampleValueObject { get; init; }

    public int? RemoteCode { get; init; }

    [BsonIgnore]
    IExampleValueObject IExample.ExampleValueObject => ExampleValueObject;
}