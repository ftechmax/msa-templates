using ApplicationName.Shared.Aggregates;
using ProtoBuf;

namespace ApplicationName.Shared.Projections;

[ProtoContract]
public record ExampleEntityProjection : IExampleEntity
{
    [ProtoMember(1)]
    public Guid Id { get; init; }

    [ProtoMember(2)]
    public string Name { get; init; }

    [ProtoMember(3)]
    public float SomeValue { get; init; }
}