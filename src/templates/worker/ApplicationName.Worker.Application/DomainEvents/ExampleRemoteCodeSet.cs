namespace ApplicationName.Worker.Application.DomainEvents;

public record ExampleRemoteCodeSet : DomainEvent
{
    public ExampleRemoteCodeSet(Guid aggregateId, int remoteCode)
    {
        Id = aggregateId;
        Timestamp = DateTime.UtcNow;
        RemoteCode = remoteCode;
    }

    public int RemoteCode { get; init; }
}