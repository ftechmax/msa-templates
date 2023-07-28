using MongoDB.Bson.Serialization.Attributes;
using System;

namespace ApplicationName.Worker.Application.Documents;

public abstract class DocumentBase
{
    [BsonId]
    public Guid Id { get; protected set; }

    public DateTime Created { get; protected set; }

    public DateTime Updated { get; protected set; }
}