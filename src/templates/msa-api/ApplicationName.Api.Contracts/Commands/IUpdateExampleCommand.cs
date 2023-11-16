using System;
using ApplicationName.Worker.Contracts.Events;

// ReSharper disable once CheckNamespace
namespace ApplicationName.Worker.Contracts.Commands;

public interface IUpdateExampleCommand
{
    Guid CorrelationId { get; }

    Guid Id { get; }

    string Description { get; }

    IExampleValueObjectEventData ExampleValueObject { get; }
}