using ApplicationName.Shared.Aggregates;
using ApplicationName.Shared.Commands;
using ApplicationName.Worker.Application.DomainEvents;
using ApplicationName.Worker.Contracts.Commands;
using ArgDefender;
using MongoDB.Bson.Serialization.Attributes;

namespace ApplicationName.Worker.Application.Documents;

public sealed class ExampleDocument : DocumentBase, IExample
{
    [BsonConstructor]
    private ExampleDocument()
    {
    }

    public ExampleDocument(ICreateExampleCommand command)
        : this()
    {
        Guard.Argument(command).NotNull();
        Guard.Argument(command.Name).NotNull().NotWhiteSpace();
        Guard.Argument(command.Description).NotNull().NotWhiteSpace();
        Guard.Argument(command.ExampleValueObject).NotNull();

        Id = Guid.NewGuid();
        Created = Updated = DateTime.UtcNow;
        Name = command.Name;
        Description = command.Description;
        ExampleValueObject = new ExampleValueObject(command.ExampleValueObject);
    }

    public ExampleUpdated Update(IUpdateExampleCommand command)
    {
        Guard.Argument(command).NotNull();
        Guard.Argument(command.Description).NotNull().NotWhiteSpace();
        Guard.Argument(command.ExampleValueObject).NotNull();

        Description = command.Description;
        ExampleValueObject = new ExampleValueObject(command.ExampleValueObject);

        return new ExampleUpdated(this);
    }

    public ExampleEntityAdded AddExampleEntity(IAddExampleEntityCommand command)
    {
        Guard.Argument(command).NotNull();

        var entity = new ExampleEntity(command);
        Examples.Add(entity);

        return new ExampleEntityAdded(Id, entity);
    }

    public ExampleEntityUpdated UpdateExampleEntity(IUpdateExampleEntityCommand command)
    {
        Guard.Argument(command).NotNull();
        Guard.Argument(command.EntityId).NotDefault();

        var entity = Examples.Single(i => i.Id == command.EntityId);
        entity.Update(command);

        return new ExampleEntityUpdated(Id, entity);
    }

    public ExampleRemoteCodeSet SetRemoteCode(ISetExampleRemoteCodeCommand command)
    {
        Guard.Argument(command).NotNull();
        Guard.Argument(command.RemoteCode).NotDefault().NotNegative();

        RemoteCode = command.RemoteCode;

        return new ExampleRemoteCodeSet(Id, RemoteCode.Value);
    }

    public static (ExampleDocument aggregate, ExampleCreated domainEvent) Create(ICreateExampleCommand command)
    {
        var document = new ExampleDocument(command);
        return (document, new ExampleCreated(document));
    }

    public string Name { get; init; }

    public string Description { get; private set; }

    public List<ExampleEntity> Examples { get; init; } = [];

    public ExampleValueObject ExampleValueObject { get; private set; }

    public int? RemoteCode { get; private set; }

    [BsonIgnore]
    IReadOnlyCollection<IExampleEntity> IExample.Examples => Examples;

    [BsonIgnore]
    IExampleValueObject IExample.ExampleValueObject => ExampleValueObject;
}