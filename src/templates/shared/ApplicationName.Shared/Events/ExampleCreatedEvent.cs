namespace ApplicationName.Shared.Events;

public record ExampleCreatedEvent
{
    public Guid CorrelationId { get; init; }

    public Guid Id { get; init; }

    public string Name { get; init; }

    public string Description { get; init; }

    public ExampleValueObjectEventData ExampleValueObject { get; init; }
}