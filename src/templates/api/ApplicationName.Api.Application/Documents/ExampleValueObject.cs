using ApplicationName.Shared.Aggregates;

namespace ApplicationName.Api.Application.Documents;

public sealed class ExampleValueObject : IExampleValueObject
{
    public required string Code { get; init; }

    public double Value { get; init; }
}