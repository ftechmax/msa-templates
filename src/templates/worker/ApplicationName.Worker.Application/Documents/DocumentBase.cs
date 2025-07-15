using ApplicationName.Shared.Aggregates;
using MongoDB.Bson.Serialization.Attributes;

namespace ApplicationName.Worker.Application.Documents;

public abstract class DocumentBase : IAggregate
{
    [BsonId]
    public Guid Id { get; protected init; }

    public DateTime Created { get; protected init; }

    public DateTime Updated { get; protected init; }
}