namespace ApplicationName.Api.Contracts;

public record DomainFault(Guid CorrelationId, string Message, string TraceId);