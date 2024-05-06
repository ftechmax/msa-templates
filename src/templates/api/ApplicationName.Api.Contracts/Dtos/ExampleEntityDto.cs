namespace ApplicationName.Api.Contracts.Dtos;

public record ExampleEntityDto
{
    public Guid Id { get; init; }

    public string Name { get; init; }

    public float SomeValue { get; init; }
}