namespace ApplicationName.Shared.Events;

public record ExampleUpdatedEvent
{
    public Guid CorrelationId { get; init; }

    public Guid Id { get; init; }

    public required string Description { get; init; }

    public required ExampleValueObjectEventData ExampleValueObject { get; init; }
}