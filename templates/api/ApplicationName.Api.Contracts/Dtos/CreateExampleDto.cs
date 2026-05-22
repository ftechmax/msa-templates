namespace ApplicationName.Api.Contracts.Dtos;

public record CreateExampleDto
{
    public required Guid CorrelationId { get; init; }

    public required string Name { get; init; }

    public required string Description { get; init; }

    public required ExampleValueObjectDto ExampleValueObject { get; init; }
}