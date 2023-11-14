using System;

// ReSharper disable once CheckNamespace
namespace Other.Worker.Contracts.Commands;

public interface IExternalEvent
{
    Guid CorrelationId { get; }

    Guid Id { get; }

    int Code { get; }
}