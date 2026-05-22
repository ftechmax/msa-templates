// ReSharper disable once CheckNamespace
namespace Other.Worker.Contracts.Commands;

public record ExternalEvent
{
    public Guid CorrelationId { get; init; }

    public Guid Id { get; init; }

    public int Code { get; init; }
}