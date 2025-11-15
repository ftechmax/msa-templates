using ApplicationName.Shared.Aggregates;
using ProtoBuf;

namespace ApplicationName.Shared.Projections;

[ProtoContract]
public record ExampleProjection : ProjectionBase, IExample
{
    [ProtoMember(1)]
    public required string Name { get; init; }

    [ProtoMember(2)]
    public required string Description { get; init; }

    [ProtoMember(3)]
    public ExampleValueObjectProjection ExampleValueObject { get; init; }

    [ProtoMember(4)]
    public int? RemoteCode { get; init; }

    IExampleValueObject IExample.ExampleValueObject => ExampleValueObject;
}