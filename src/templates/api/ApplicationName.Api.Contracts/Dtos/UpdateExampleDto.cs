namespace ApplicationName.Api.Contracts.Dtos;

public record UpdateExampleDto
{
    public Guid CorrelationId { get; init; }

    public required string Description { get; init; }

    public required ExampleValueObjectDto ExampleValueObject { get; init; }
}