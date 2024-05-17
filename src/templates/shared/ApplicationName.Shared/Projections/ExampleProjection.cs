using System.Collections.ObjectModel;
using ApplicationName.Shared.Aggregates;
using ProtoBuf;

namespace ApplicationName.Shared.Projections;

[ProtoContract]
public record ExampleProjection : ProjectionBase, IExample
{
    [ProtoMember(1)]
    public string Name { get; init; }

    [ProtoMember(2)]
    public string Description { get; init; }

    [ProtoMember(3)]
    public Collection<ExampleEntityProjection> Examples { get; init; } = [];

    [ProtoMember(4)]
    public ExampleValueObjectProjection ExampleValueObject { get; init; }

    [ProtoMember(5)]
    public int? RemoteCode { get; init; }

    IReadOnlyCollection<IExampleEntity> IExample.Examples => Examples;

    IExampleValueObject IExample.ExampleValueObject => ExampleValueObject;
}