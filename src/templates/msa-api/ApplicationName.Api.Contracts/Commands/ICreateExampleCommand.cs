using System;
using ApplicationName.Worker.Contracts.Events;

// ReSharper disable once CheckNamespace
namespace ApplicationName.Worker.Contracts.Commands;

public interface ICreateExampleCommand
{
    Guid CorrelationId { get; }

    string Name { get; }

    string Description { get; }

    IExampleValueObjectEventData ExampleValueObject { get; }
}