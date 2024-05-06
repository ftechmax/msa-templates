using ApplicationName.Shared.Commands;

namespace ApplicationName.Api.Application.Commands;

public record UpdateExampleEntityCommand : IUpdateExampleEntityCommand
{
    public Guid CorrelationId { get; init; }

    public Guid Id { get; init; }

    public Guid EntityId { get; init; }

    public float SomeValue { get; init; }
}