namespace ApplicationName.Shared.Events;

public record ExampleValueObjectEventData
{
    public required string Code { get; init; }

    public double Value { get; init; }
}