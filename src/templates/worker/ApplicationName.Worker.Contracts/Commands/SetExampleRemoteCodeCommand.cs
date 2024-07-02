namespace ApplicationName.Worker.Contracts.Commands;

public record SetExampleRemoteCodeCommand
{
    public Guid CorrelationId { get; init; }

    public Guid Id { get; init; }

    public int RemoteCode { get; init; }
}