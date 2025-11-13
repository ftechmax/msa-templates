namespace ApplicationName.Worker.Application.DomainEvents;

public abstract record DomainEvent
{
    public Guid Id { get; protected init; }

    public DateTime Timestamp { get; protected init; }
}