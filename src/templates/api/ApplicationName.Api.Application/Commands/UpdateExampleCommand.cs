using ApplicationName.Shared.Commands;
using ApplicationName.Shared.Events;

namespace ApplicationName.Api.Application.Commands;

public record UpdateExampleCommand : IUpdateExampleCommand
{
    public Guid CorrelationId { get; init; }

    public Guid Id { get; set; }

    public string Description { get; init; }

    public ExampleValueObjectEventData ExampleValueObject { get; init; }

    IExampleValueObjectEventData IUpdateExampleCommand.ExampleValueObject => ExampleValueObject;
}