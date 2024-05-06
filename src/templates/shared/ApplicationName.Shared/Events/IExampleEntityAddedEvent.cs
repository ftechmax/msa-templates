namespace ApplicationName.Shared.Events;

public interface IExampleEntityAddedEvent
{
    Guid CorrelationId { get; }

    Guid Id { get; }

    Guid EntityId { get; }
}