namespace ApplicationName.Api.Contracts.Dtos;

public record CreateExampleDto
{
    public Guid CorrelationId { get; init; }

    public string Name { get; init; }

    public string Description { get; init; }

    public ExampleValueObjectDto ExampleValueObject { get; init; }
}