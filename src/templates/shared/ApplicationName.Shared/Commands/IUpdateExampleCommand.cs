using System;
using ApplicationName.Shared.Events;

namespace ApplicationName.Shared.Commands;

public interface IUpdateExampleCommand
{
    Guid CorrelationId { get; }

    Guid Id { get; }

    string Description { get; }

    IExampleValueObjectEventData ExampleValueObject { get; }
}