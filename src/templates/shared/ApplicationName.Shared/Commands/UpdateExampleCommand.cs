using ApplicationName.Shared.Events;

namespace ApplicationName.Shared.Commands;

public record UpdateExampleCommand
{
    public Guid CorrelationId { get; init; }

    public Guid Id { get; init; }

    public string Description { get; init; }

    public ExampleValueObjectEventData ExampleValueObject { get; init; }
}