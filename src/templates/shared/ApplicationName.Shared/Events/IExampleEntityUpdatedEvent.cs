using System;

namespace ApplicationName.Shared.Events;

public interface IExampleEntityUpdatedEvent
{
    Guid CorrelationId { get; }
}