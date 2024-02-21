using System;

namespace ApplicationName.Shared.Commands;

public interface ISetExampleRemoteCodeCommand
{
    Guid CorrelationId { get; }

    Guid Id { get; }

    int RemoteCode { get; }
}