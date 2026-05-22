using ProtoBuf;

namespace ApplicationName.Api.Contracts.Dtos;

[ProtoContract]
public record ExampleValueObjectDto
{
    [ProtoMember(1)]
    public required string Code { get; init; }

    [ProtoMember(2)]
    public double Value { get; init; }
}