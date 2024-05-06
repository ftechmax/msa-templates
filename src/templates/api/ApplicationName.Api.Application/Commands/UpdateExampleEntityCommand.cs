using ApplicationName.Shared.Commands;

namespace ApplicationName.Api.Application.Commands;

public record UpdateExampleEntityCommand : IUpdateExampleEntityCommand
{
    public Guid CorrelationId { get; init; }

    public Guid Id { get; set; }

    public Guid EntityId { get; set; }

    public float SomeValue { get; init; }
}