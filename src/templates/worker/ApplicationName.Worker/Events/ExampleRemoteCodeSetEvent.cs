using System;
using ApplicationName.Shared.Events;

namespace ApplicationName.Worker.Events;

public class ExampleRemoteCodeSetEvent : IExampleRemoteCodeSetEvent
{
    public Guid CorrelationId { get; set; }

    public Guid Id { get; set; }

    public int RemoteCode { get; set; }
}