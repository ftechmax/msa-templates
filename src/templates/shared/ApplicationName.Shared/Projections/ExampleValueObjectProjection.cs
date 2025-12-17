using ApplicationName.Shared.Aggregates;
using ProtoBuf;

namespace ApplicationName.Shared.Projections;

[ProtoContract]
public record ExampleValueObjectProjection : IExampleValueObject
{
    [ProtoMember(1)]
    public required string Code { get; init; }

    [ProtoMember(2)]
    public double Value { get; init; }
}