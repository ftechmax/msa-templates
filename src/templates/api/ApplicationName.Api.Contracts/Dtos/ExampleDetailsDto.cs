using ProtoBuf;

namespace ApplicationName.Api.Contracts.Dtos;

[ProtoContract]
public record ExampleDetailsDto
{
    [ProtoMember(1)]
    public Guid Id { get; init; }

    [ProtoMember(2)]
    public string Name { get; init; }

    [ProtoMember(3)]
    public string Description { get; init; }

    [ProtoMember(4)]
    public ExampleValueObjectDto ExampleValueObject { get; init; }

    [ProtoMember(6)]
    public int? RemoteCode { get; init; }
}