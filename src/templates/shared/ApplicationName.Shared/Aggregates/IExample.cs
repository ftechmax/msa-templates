namespace ApplicationName.Shared.Aggregates;

public interface IExample : IAggregate
{
    string Name { get; }

    string Description { get; }

    IReadOnlyCollection<IExampleEntity> Examples { get; }

    IExampleValueObject ExampleValueObject { get; }

    int? RemoteCode { get; }
}