using System;

// ReSharper disable once CheckNamespace
namespace ApplicationName.Worker.Contracts.Commands;

public interface IUpdateExampleEntityCommand
{
    Guid CorrelationId { get; }

    Guid Id { get; }

    Guid EntityId { get; }

    float SomeValue { get; }
}