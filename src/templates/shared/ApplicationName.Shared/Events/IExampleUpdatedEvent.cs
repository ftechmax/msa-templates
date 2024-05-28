namespace ApplicationName.Shared.Events;

public interface IExampleUpdatedEvent
{
    Guid CorrelationId { get; }

    Guid Id { get; }

    string Description { get; }

    IExampleValueObjectEventData ExampleValueObject { get; }
}