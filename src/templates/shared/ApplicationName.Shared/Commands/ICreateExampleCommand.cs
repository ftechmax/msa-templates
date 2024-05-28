using ApplicationName.Shared.Events;

namespace ApplicationName.Shared.Commands;

public interface ICreateExampleCommand
{
    Guid CorrelationId { get; }

    string Name { get; }

    string Description { get; }

    IExampleValueObjectEventData ExampleValueObject { get; }
}