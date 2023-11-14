using System;

namespace ApplicationName.Worker.Contracts.Commands;

public interface IAddExampleEntityCommand
{
    Guid CorrelationId { get; }

    Guid Id { get; }

    string Name { get; }

    float SomeValue { get; }
}