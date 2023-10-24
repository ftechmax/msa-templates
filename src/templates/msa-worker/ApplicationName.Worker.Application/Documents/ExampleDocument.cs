using System;
using ApplicationName.Worker.Contracts.Specifications;
using ArgDefender;
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
        Guard.Argument(spec).NotNull();

        Id = Guid.NewGuid();
        Created = Updated = DateTime.UtcNow;

        Update(spec);
    }

    public void Update(IExampleSpecification spec)
    {
        Guard.Argument(spec).NotNull();
        Guard.Argument(spec.Name).NotNull().NotWhiteSpace();

        Name = spec.Name;
    }

    public string Name { get; private set; }
}