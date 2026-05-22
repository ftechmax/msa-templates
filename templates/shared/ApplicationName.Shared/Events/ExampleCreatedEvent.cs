namespace ApplicationName.Shared.Events;

public record ExampleCreatedEvent
{
    public Guid CorrelationId { get; init; }

    public Guid Id { get; init; }

    public required string Name { get; init; }

    public required string Description { get; init; }

    public required ExampleValueObjectEventData ExampleValueObject { get; init; }
}