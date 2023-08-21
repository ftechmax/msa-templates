using System;
using ApplicationName.Worker.Contracts.Specifications;
using CommunityToolkit.Diagnostics;
using MongoDB.Bson.Serialization.Attributes;

namespace ApplicationName.Worker.Application.Documents;

public sealed class ExampleDocument : DocumentBase
{
    [BsonConstructor]
    private ExampleDocument()
    {
    }

    public ExampleDocument(IExampleSpecification spec)
        : this()
    {
        Guard.IsNotNull(spec);

        Id = Guid.NewGuid();
        Created = Updated = DateTime.UtcNow;

        Update(spec);
    }

    public void Update(IExampleSpecification spec)
    {
        Guard.IsNotNull(spec);
        Guard.IsNotNullOrWhiteSpace(spec.Name);

        Name = spec.Name;
    }

    public string Name { get; private set; }
}