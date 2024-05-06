namespace ApplicationName.Shared.Events;

public interface IExampleEntityUpdatedEvent
{
    Guid CorrelationId { get; }

    Guid Id { get; }

    Guid EntityId { get; }
}