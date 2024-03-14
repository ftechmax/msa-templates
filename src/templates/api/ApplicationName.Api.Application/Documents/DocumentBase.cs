using ApplicationName.Shared.Aggregates;
using MongoDB.Bson.Serialization.Attributes;

namespace ApplicationName.Api.Application.Documents;

public abstract class DocumentBase : IAggregate
{
    [BsonId]
    public Guid Id { get; protected set; }

    public DateTime Created { get; protected set; }

    public DateTime Updated { get; protected set; }
}