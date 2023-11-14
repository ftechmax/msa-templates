using System;
using ApplicationName.Worker.Contracts.Events;

namespace ApplicationName.Worker.Events;

public class ExampleEntityAddedEvent : IExampleEntityAddedEvent
{
    public Guid CorrelationId { get; set; }
}