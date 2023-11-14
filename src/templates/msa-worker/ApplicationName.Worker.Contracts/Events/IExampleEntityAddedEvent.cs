using System;

namespace ApplicationName.Worker.Contracts.Events;

public interface IExampleEntityAddedEvent
{
    Guid CorrelationId { get; }
}