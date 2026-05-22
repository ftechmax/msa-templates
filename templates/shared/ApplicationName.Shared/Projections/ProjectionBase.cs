using ApplicationName.Shared.Aggregates;
using ProtoBuf;

namespace ApplicationName.Shared.Projections;

[ProtoContract]
[ProtoInclude(100, typeof(ExampleProjection))]
public record ProjectionBase : IAggregate
{
    [ProtoMember(1)]
    public Guid Id { get; init; }

    [ProtoMember(2)]
    public DateTime Created { get; init; }

    [ProtoMember(3)]
    public DateTime Updated { get; init; }
}