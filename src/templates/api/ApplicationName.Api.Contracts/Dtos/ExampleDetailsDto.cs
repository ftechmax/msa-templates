using ProtoBuf;

namespace ApplicationName.Api.Contracts.Dtos;

[ProtoContract]
public record ExampleDetailsDto
{
    [ProtoMember(1)]
    public required Guid Id { get; init; }

    [ProtoMember(2)]
    public required string Name { get; init; }

    [ProtoMember(3)]
    public required string Description { get; init; }

    [ProtoMember(4)]
    public required ExampleValueObjectDto ExampleValueObject { get; init; }

    [ProtoMember(6)]
    public int? RemoteCode { get; init; }
}