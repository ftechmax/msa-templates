using System;

namespace ApplicationName.Worker.Contracts.Events;

public interface IExampleRemoteCodeSetEvent
{
    Guid CorrelationId { get; }

    Guid Id { get; }

    int RemoteCode { get; }
}