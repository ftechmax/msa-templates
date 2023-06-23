using ApplicationName.Api.Application.Commands;
using ApplicationName.Api.Application.Documents;
using ApplicationName.Api.Contracts.Dtos;
using AutoMapper;

namespace ApplicationName.Api;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CreateExampleDto, ExampleCommand>();
        CreateMap<ExampleDocument, ExampleResultDto>();
    }
}