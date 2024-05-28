namespace ApplicationName.Api.Contracts.Dtos;

public record UpdateExampleEntityDto
{
    public Guid CorrelationId { get; init; }

    public float SomeValue { get; init; }
}