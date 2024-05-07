namespace ApplicationName.Worker.Application.DomainEvents;

public abstract record DomainEvent
{
    public Guid Id { get; init; }

    public DateTime Timestamp { get; init; }
}