using ApplicationName.Worker.Application.Documents;

namespace ApplicationName.Worker.Application.DomainEvents;

public class ExampleEntityAdded : DomainEvent
{
    public ExampleEntityAdded(Guid aggregateId, ExampleEntity entity)
    {
        Id = aggregateId;
        Timestamp = DateTime.UtcNow;
        ExampleId = entity.Id;
        Name = entity.Name;
        SomeValue = entity.SomeValue;
    }

    public Guid ExampleId { get; set; }

    public string Name { get; set; }

    public float SomeValue { get; set; }
}