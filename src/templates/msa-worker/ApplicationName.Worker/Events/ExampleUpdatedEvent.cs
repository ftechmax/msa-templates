using System;
using ApplicationName.Shared.Events;

namespace ApplicationName.Worker.Events;

public class ExampleUpdatedEvent : IExampleUpdatedEvent
{
    public Guid CorrelationId { get; set; }

    public Guid Id { get; set; }

    public string Description { get; set; }

    public ExampleValueObjectEventData ExampleValueObject { get; set; }

    IExampleValueObjectEventData IExampleUpdatedEvent.ExampleValueObject => ExampleValueObject;
}