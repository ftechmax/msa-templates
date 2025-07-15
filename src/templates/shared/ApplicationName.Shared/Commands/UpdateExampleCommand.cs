using ApplicationName.Shared.Events;

namespace ApplicationName.Shared.Commands;

public record UpdateExampleCommand
{
    public Guid CorrelationId { get; init; }

    public Guid Id { get; init; }

    public required string Description { get; init; }

    public required ExampleValueObjectEventData ExampleValueObject { get; init; }
}