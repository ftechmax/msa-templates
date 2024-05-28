using ApplicationName.Shared.Events;

namespace ApplicationName.Worker.Events;

public class ExampleCreatedEvent : IExampleCreatedEvent
{
    public Guid CorrelationId { get; set; }

    public Guid Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public ExampleValueObjectEventData ExampleValueObject { get; set; }

    IExampleValueObjectEventData IExampleCreatedEvent.ExampleValueObject => ExampleValueObject;
}