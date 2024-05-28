using ApplicationName.Shared.Events;

namespace ApplicationName.Api.Application.Commands;

public record ExampleValueObjectEventData : IExampleValueObjectEventData
{
    public string Code { get; init; }

    public double Value { get; init; }
}