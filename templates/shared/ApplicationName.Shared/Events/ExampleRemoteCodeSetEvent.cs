namespace ApplicationName.Shared.Events;

public record ExampleRemoteCodeSetEvent
{
    public Guid CorrelationId { get; init; }

    public Guid Id { get; init; }

    public int RemoteCode { get; init; }
}