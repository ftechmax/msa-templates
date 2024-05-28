namespace ApplicationName.Shared.Commands;

public interface IUpdateExampleEntityCommand
{
    Guid CorrelationId { get; }

    Guid Id { get; }

    Guid EntityId { get; }

    float SomeValue { get; }
}