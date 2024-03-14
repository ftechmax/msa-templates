using ApplicationName.Worker.Application.Documents;

namespace ApplicationName.Worker.Application.DomainEvents;

public class ExampleRemoteCodeSet : DomainEvent
{
    public ExampleRemoteCodeSet(ExampleDocument aggregate)
    {
        Id = aggregate.Id;
        Timestamp = DateTime.UtcNow;
        RemoteCode = aggregate.RemoteCode!.Value;
    }

    public int RemoteCode { get; set; }
}