using System.Collections.ObjectModel;
using ApplicationName.Shared.Aggregates;
using ProtoBuf;

namespace ApplicationName.Shared.Projections;

[ProtoContract]
public record ExampleProjection : IExample
{
    [ProtoMember(1)]
    public Guid Id { get; init; }

    [ProtoMember(2)]
    public DateTime Created { get; init; }

    [ProtoMember(3)]
    public DateTime Updated { get; init; }

    [ProtoMember(4)]
    public string Name { get; init; }

    [ProtoMember(5)]
    public string Description { get; init; }

    [ProtoMember(6)]
    public Collection<ExampleEntityProjection> Examples { get; init; } = [];

    [ProtoMember(7)]
    public ExampleValueObjectProjection ExampleValueObject { get; init; }

    [ProtoMember(8)]
    public int? RemoteCode { get; init; }

    IReadOnlyCollection<IExampleEntity> IExample.Examples => Examples;

    IExampleValueObject IExample.ExampleValueObject => ExampleValueObject;
}