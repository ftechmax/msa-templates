using System;

// ReSharper disable once CheckNamespace
namespace ApplicationName.Worker.Contracts.Commands;

public interface ISetExampleRemoteCodeCommand
{
    Guid CorrelationId { get; }

    Guid Id { get; }

    int RemoteCode { get; }
}