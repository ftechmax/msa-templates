using System;

namespace ApplicationName.Shared.Aggregates;

public interface IExampleEntity
{
    Guid Id { get; }

    string Name { get; }

    float SomeValue { get; }
}