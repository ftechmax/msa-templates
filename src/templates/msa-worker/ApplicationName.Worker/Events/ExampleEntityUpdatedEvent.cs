using System;
using ApplicationName.Worker.Contracts.Events;

namespace ApplicationName.Worker.Events;

public class ExampleEntityUpdatedEvent : IExampleEntityUpdatedEvent
{
    public Guid CorrelationId { get; set; }
}