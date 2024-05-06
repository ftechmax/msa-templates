namespace ApplicationName.Api.Contracts.Dtos;

public record UpdateExampleDto
{
    public Guid CorrelationId { get; init; }

    public Guid Id { get; init; }

    public string Description { get; init; }

    public ExampleValueObjectDto ExampleValueObject { get; init; }
}