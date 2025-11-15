using ApplicationName.Shared.Aggregates;
using ProtoBuf;

namespace ApplicationName.Shared.Projections;

[ProtoContract]
public record ExampleValueObjectProjection : IExampleValueObject
{
    [ProtoMember(1)]
    public string Code { get; init; }

    [ProtoMember(2)]
    public double Value { get; init; }
}