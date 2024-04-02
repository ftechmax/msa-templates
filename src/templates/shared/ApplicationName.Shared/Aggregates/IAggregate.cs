namespace ApplicationName.Shared.Aggregates;

public interface IAggregate
{
    Guid Id { get; }

    DateTime Created { get; }

    DateTime Updated { get; }
}