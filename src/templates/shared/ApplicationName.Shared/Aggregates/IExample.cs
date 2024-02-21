using System.Collections.Generic;

namespace ApplicationName.Shared.Aggregates;

public interface IExample
{
    string Name { get; }

    string Description { get; }

    IReadOnlyCollection<IExampleEntity> Examples { get; }

    IExampleValueObject ExampleValueObject { get; }

    int? RemoteCode { get; }
}