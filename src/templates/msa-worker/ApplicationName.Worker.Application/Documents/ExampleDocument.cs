using ApplicationName.Worker.Contracts.Specifications;
using Dawn;
using MongoDB.Bson.Serialization.Attributes;
using System;

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
        Guard.Argument(spec, nameof(spec)).NotNull();

        Id = Guid.NewGuid();
        Created = Updated = DateTime.UtcNow;

        Update(spec);
    }

    public void Update(IExampleSpecification spec)
    {
        Guard.Argument(spec, nameof(spec)).NotNull();
        Guard.Argument(spec.Name, nameof(spec.Name)).NotNull().NotWhiteSpace();

        Name = spec.Name;
    }

    public string Name { get; private set; }
}