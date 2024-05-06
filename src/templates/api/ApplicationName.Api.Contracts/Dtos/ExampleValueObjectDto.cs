namespace ApplicationName.Api.Contracts.Dtos;

public record ExampleValueObjectDto
{
    public string Code { get; init; }

    public double Value { get; init; }
}