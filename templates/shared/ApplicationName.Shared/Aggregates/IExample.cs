namespace ApplicationName.Shared.Aggregates;

public interface IExample : IAggregate
{
    string Name { get; }

    string Description { get; }

    IExampleValueObject ExampleValueObject { get; }
}