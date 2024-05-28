namespace ApplicationName.Api.Contracts.Dtos;

public record AddExampleEntityDto
{
    public Guid CorrelationId { get; init; }

    public string Name { get; init; }

    public float SomeValue { get; init; }
}