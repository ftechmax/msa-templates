using System;

namespace ApplicationName.Worker.Contracts.Events;

public interface IExampleEntityUpdatedEvent
{
    Guid CorrelationId { get; }
}