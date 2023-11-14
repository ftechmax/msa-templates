using System;

namespace ApplicationName.Worker.Application.DomainEvents;

public abstract class DomainEvent
{
    public Guid Id { get; set; }

    public DateTime Timestamp { get; set; }
}