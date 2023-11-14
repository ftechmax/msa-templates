using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationName.Worker.Application.DomainEvents;
using ApplicationName.Worker.Contracts.Commands;
using ArgDefender;
using MongoDB.Bson.Serialization.Attributes;

namespace ApplicationName.Worker.Application.Documents;

public sealed class ExampleDocument : DocumentBase
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
        _examples.Add(entity);

        return new ExampleEntityAdded(Id, entity);
    }

    public ExampleEntityUpdated UpdateExampleEntity(IUpdateExampleEntityCommand command)
    {
        Guard.Argument(command).NotNull();
        Guard.Argument(command.EntityId).NotDefault();

        var entity = _examples.Single(i => i.Id == command.EntityId);
        entity.Update(command);

        return new ExampleEntityUpdated(Id, entity);
    }

    public ExampleRemoteCodeSet SetRemoteCode(ISetExampleRemoteCodeCommand command)
    {
        Guard.Argument(command).NotNull();
        Guard.Argument(command.RemoteCode).NotDefault().NotNegative();

        RemoteCode = command.RemoteCode;

        return new ExampleRemoteCodeSet(this);
    }

    public static (ExampleDocument aggregate, ExampleCreated domainEvent) Create(ICreateExampleCommand command)
    {
        var document = new ExampleDocument(command);
        return (document, new ExampleCreated(document));
    }

    public string Name { get; private set; }

    public string Description { get; private set; }

    [BsonElement("Examples")] private readonly List<ExampleEntity> _examples = new();

    public IReadOnlyCollection<ExampleEntity> Examples => _examples.AsReadOnly();

    public ExampleValueObject ExampleValueObject { get; private set; }

    public int? RemoteCode { get; private set; }
}