using ApplicationName.Api.Contracts.Documents;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace ApplicationName.Api.Application.Documents;

public abstract class DocumentBase : IDocumentBase
{
    [BsonId]
    public Guid Id { get; protected set; }

    public DateTime Created { get; protected set; }

    public DateTime Updated { get; protected set; }
}