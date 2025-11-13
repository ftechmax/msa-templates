using ApplicationName.Shared.Aggregates;
using MongoDB.Bson.Serialization.Attributes;

namespace ApplicationName.Worker.Application.Documents;

public abstract class DocumentBase : IAggregate
{
    [BsonId]
    public Guid Id { get; init; }

    public DateTime Created { get; init; }

    public DateTime Updated { get; init; }
}