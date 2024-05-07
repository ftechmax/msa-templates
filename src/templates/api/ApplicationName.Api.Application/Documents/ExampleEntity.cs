using ApplicationName.Shared.Aggregates;
using MongoDB.Bson.Serialization.Attributes;

namespace ApplicationName.Api.Application.Documents;

public sealed class ExampleEntity : IExampleEntity
{
    [BsonConstructor]
    private ExampleEntity()
    {
    }

    public Guid Id { get; init; }

    public string Name { get; init; }

    public float SomeValue { get; init; }
}