using ApplicationName.Shared.Events;

namespace ApplicationName.Shared.Commands;

public record CreateExampleCommand
{
    public Guid CorrelationId { get; init; }

    public string Name { get; init; }

    public string Description { get; init; }

    public ExampleValueObjectEventData ExampleValueObject { get; init; }
}