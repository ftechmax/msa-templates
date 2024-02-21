using System;
using ApplicationName.Shared.Commands;

namespace ApplicationName.Api.Application.Commands;

public class AddExampleEntityCommand : IAddExampleEntityCommand
{
    public Guid CorrelationId { get; set; }

    public Guid Id { get; set; }

    public string Name { get; set; }

    public float SomeValue { get; set; }
}