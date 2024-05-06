using ApplicationName.Shared.Commands;

namespace ApplicationName.Api.Application.Commands;

public record AddExampleEntityCommand : IAddExampleEntityCommand
{
    public Guid CorrelationId { get; init; }

    public Guid Id { get; init; }

    public string Name { get; init; }

    public float SomeValue { get; init; }
}