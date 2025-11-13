using ProtoBuf;

namespace ApplicationName.Api.Contracts.Dtos;

[ProtoContract]
public record ExampleCollectionDto
{
    [ProtoMember(1)]
    public required Guid Id { get; init; }

    [ProtoMember(2)]
    public DateTime Created { get; init; }

    [ProtoMember(3)]
    public string Name { get; init; }

    [ProtoMember(4)]
    public string Description { get; init; }
}