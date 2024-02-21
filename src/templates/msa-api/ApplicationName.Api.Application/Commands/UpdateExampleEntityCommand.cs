using System;
using ApplicationName.Shared.Commands;

namespace ApplicationName.Api.Application.Commands;

public class UpdateExampleEntityCommand : IUpdateExampleEntityCommand
{
    public Guid CorrelationId { get; set; }

    public Guid Id { get; set; }

    public Guid EntityId { get; set; }

    public float SomeValue { get; set; }
}