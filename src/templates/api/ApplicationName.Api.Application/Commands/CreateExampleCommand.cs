using System;
using ApplicationName.Shared.Commands;
using ApplicationName.Shared.Events;

namespace ApplicationName.Api.Application.Commands;

public class CreateExampleCommand : ICreateExampleCommand
{
    public Guid CorrelationId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public ExampleValueObjectEventData ExampleValueObject { get; set; }

    IExampleValueObjectEventData ICreateExampleCommand.ExampleValueObject => ExampleValueObject;
}