namespace ApplicationName.Shared.Events;

public interface IExampleEntityAddedEvent
{
    Guid CorrelationId { get; }
}