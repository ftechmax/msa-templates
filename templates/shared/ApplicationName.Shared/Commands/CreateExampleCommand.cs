using ApplicationName.Shared.Events;

namespace ApplicationName.Shared.Commands;

public record CreateExampleCommand
{
    public Guid CorrelationId { get; init; }

    public required string Name { get; init; }

    public required string Description { get; init; }

    public required ExampleValueObjectEventData ExampleValueObject { get; init; }
}