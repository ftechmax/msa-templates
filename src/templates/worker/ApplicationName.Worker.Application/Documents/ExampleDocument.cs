using ApplicationName.Shared.Aggregates;
using ApplicationName.Shared.Commands;
using ApplicationName.Worker.Application.DomainEvents;
using ApplicationName.Worker.Contracts.Commands;
using ArgDefender;
using MongoDB.Bson.Serialization.Attributes;

namespace ApplicationName.Worker.Application.Documents;

public sealed class ExampleDocument : DocumentBase, IExample
{
    // Suppress CS8618: Non-nullable property must contain a non-null value when exiting constructor.
    // This constructor is used by the database driver for deserialization and properties are set via reflection.
#pragma warning disable CS8618
    [BsonConstructor]
    private ExampleDocument()
    {
    }
#pragma warning restore CS8618

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

    public string Name { get; }

    public string Description { get; private set; }

    public ExampleValueObject ExampleValueObject { get; private set; }

    public int? RemoteCode { get; private set; }

    [BsonIgnore]
    IExampleValueObject IExample.ExampleValueObject => ExampleValueObject;
}