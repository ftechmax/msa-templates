using ProtoBuf;

namespace ApplicationName.Api.Contracts.Dtos;

[ProtoContract]
public class ExampleDetailsDto
{
    [ProtoMember(1)]
    public string Name { get; set; }
}