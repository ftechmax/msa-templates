namespace ApplicationName.Shared.Events;

public interface IExampleRemoteCodeSetEvent
{
    Guid CorrelationId { get; }

    Guid Id { get; }

    int RemoteCode { get; }
}