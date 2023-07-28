using ProtoBuf;

namespace ApplicationName.Api.Contracts.Dtos;

[ProtoContract]
public class ExampleResultDto
{
    [ProtoMember(1)]
    public string Name { get; set; }
}