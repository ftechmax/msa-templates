namespace ApplicationName.Shared.Events;

public interface IExampleCreatedEvent
{
    Guid CorrelationId { get; }

    Guid Id { get; }

    string Name { get; }

    string Description { get; }

    IExampleValueObjectEventData ExampleValueObject { get; }
}