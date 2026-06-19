using ApplicationName.Shared.Aggregates;

namespace ApplicationName.Worker.Application.Documents;

public abstract class DocumentBase : IAggregate
{
    public Guid Id { get; init; }

    public DateTime Created { get; init; }

    public DateTime Updated { get; init; }
}