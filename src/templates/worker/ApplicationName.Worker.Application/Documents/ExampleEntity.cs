using System;
using ApplicationName.Shared.Aggregates;
using ApplicationName.Shared.Commands;
using ArgDefender;
using MongoDB.Bson.Serialization.Attributes;

namespace ApplicationName.Worker.Application.Documents;

public sealed class ExampleEntity : IExampleEntity
{
    [BsonConstructor]
    private ExampleEntity()
    {
    }

    public ExampleEntity(IAddExampleEntityCommand command)
        : this()
    {
        Guard.Argument(command).NotNull();
        Guard.Argument(command.Name).NotNull().NotWhiteSpace();
        Guard.Argument(command.SomeValue).NotNegative();

        Id = Guid.NewGuid();
        Name = command.Name;
        SomeValue = command.SomeValue;
    }

    public void Update(IUpdateExampleEntityCommand command)
    {
        Guard.Argument(command).NotNull();
        Guard.Argument(command.SomeValue).NotNegative();

        SomeValue = command.SomeValue;
    }

    [BsonElement]
    public Guid Id { get; }

    [BsonElement]
    public string Name { get; }

    public float SomeValue { get; private set; }
}