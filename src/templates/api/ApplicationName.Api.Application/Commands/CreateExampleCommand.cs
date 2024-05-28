using ApplicationName.Shared.Commands;
using ApplicationName.Shared.Events;

namespace ApplicationName.Api.Application.Commands;

public record CreateExampleCommand : ICreateExampleCommand
{
    public Guid CorrelationId { get; init; }

    public string Name { get; init; }

    public string Description { get; init; }

    public ExampleValueObjectEventData ExampleValueObject { get; init; }

    IExampleValueObjectEventData ICreateExampleCommand.ExampleValueObject => ExampleValueObject;
}