namespace ApplicationName.Shared.Events;

public record ExampleValueObjectEventData
{
    public string Code { get; init; }

    public double Value { get; init; }
}