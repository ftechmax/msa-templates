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

    public ExampleDocument(CreateExampleCommand command)
        : this()
    {
        Guard.Argument(command, nameof(command)).NotNull();
        Guard.Argument(command.Name, nameof(command.Name)).NotNull().NotWhiteSpace();
        Guard.Argument(command.Description, nameof(command.Description)).NotNull().NotWhiteSpace();
        Guard.Argument(command.ExampleValueObject, nameof(command.ExampleValueObject)).NotNull();

        Id = Guid.NewGuid();
        Created = Updated = DateTime.UtcNow;
        Name = command.Name;
        Description = command.Description;
        ExampleValueObject = new ExampleValueObject(command.ExampleValueObject);
    }

    public ExampleUpdated Handle(UpdateExampleCommand command)
    {
        Guard.Argument(command, nameof(command)).NotNull();
        Guard.Argument(command.Description, nameof(command.Description)).NotNull().NotWhiteSpace();
        Guard.Argument(command.ExampleValueObject, nameof(command.ExampleValueObject)).NotNull();

        Description = command.Description;
        ExampleValueObject = new ExampleValueObject(command.ExampleValueObject);

        return new ExampleUpdated(this);
    }

    public ExampleRemoteCodeSet Handle(SetExampleRemoteCodeCommand command)
    {
        Guard.Argument(command, nameof(command)).NotNull();
        Guard.Argument(command.RemoteCode, nameof(command.RemoteCode)).NotDefault().NotNegative();

        RemoteCode = command.RemoteCode;

        return new ExampleRemoteCodeSet(Id, RemoteCode.Value);
    }

    public static (ExampleDocument aggregate, ExampleCreated domainEvent) Create(CreateExampleCommand command)
    {
        var document = new ExampleDocument(command);
        return (document, new ExampleCreated(document));
    }

    public string Name { get; init; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public ExampleValueObject ExampleValueObject { get; private set; } = null!;

    public int? RemoteCode { get; private set; }

    [BsonIgnore]
    IExampleValueObject IExample.ExampleValueObject => ExampleValueObject;
}