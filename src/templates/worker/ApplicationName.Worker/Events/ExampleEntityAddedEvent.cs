using System;
using ApplicationName.Shared.Events;

namespace ApplicationName.Worker.Events;

public class ExampleEntityAddedEvent : IExampleEntityAddedEvent
{
    public Guid CorrelationId { get; set; }
}