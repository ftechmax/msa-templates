using System;
using ApplicationName.Worker.Contracts.Commands;

namespace ApplicationName.Api.Application.Commands;

public class SetExampleRemoteCodeCommand : ISetExampleRemoteCodeCommand
{
    public Guid CorrelationId { get; set; }

    public Guid Id { get; set; }

    public int RemoteCode { get; set; }
}