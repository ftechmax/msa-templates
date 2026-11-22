using Conveyo;

namespace ApplicationName.Worker.Contracts;

/// <summary>
/// Retrying can't help and would block the bus, so Conveyo moves the message straight to {queue}_skipped.
/// </summary>
public sealed class DocumentNotFoundException(Type documentType, Guid id)
    : MessageNotConsumedException($"{documentType.Name} {id} does not exist.");
