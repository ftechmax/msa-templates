using ProtoBuf;

namespace ApplicationName.Api.Contracts.Dtos;

[ProtoContract]
public record ExampleValueObjectDto
{
    [ProtoMember(1)]
    public string Code { get; init; }

    [ProtoMember(2)]
    public double Value { get; init; }
}