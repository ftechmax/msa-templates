using System;
using ApplicationName.Worker.Contracts.Commands;
using ApplicationName.Worker.Contracts.Events;

namespace ApplicationName.Api.Application.Commands;

public class UpdateExampleCommand : IUpdateExampleCommand
{
    public Guid CorrelationId { get; set; }

    public Guid Id { get; set; }

    public string Description { get; set; }

    public ExampleValueObjectEventData ExampleValueObject { get; set; }

    IExampleValueObjectEventData IUpdateExampleCommand.ExampleValueObject => ExampleValueObject;
}