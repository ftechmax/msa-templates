using MongoDB.Bson.Serialization.Attributes;

namespace ApplicationName.Api.Application.Documents;

public sealed class ExampleDocument : DocumentBase
{
    [BsonConstructor]
    private ExampleDocument()
    {
    }

    public string Name { get; private set; }
}