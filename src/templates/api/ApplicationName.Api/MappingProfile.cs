using System.Diagnostics.CodeAnalysis;
using ApplicationName.Api.Application.Commands;
using ApplicationName.Api.Application.Documents;
using ApplicationName.Api.Contracts.Dtos;
using AutoMapper;

namespace ApplicationName.Api;

[ExcludeFromCodeCoverage]
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CreateExampleDto, CreateExampleCommand>();
        CreateMap<UpdateExampleDto, UpdateExampleCommand>();
        CreateMap<AddExampleEntityDto, AddExampleEntityCommand>();
        CreateMap<UpdateExampleEntityDto, UpdateExampleEntityCommand>();
        CreateMap<ExampleValueObjectDto, ExampleValueObjectEventData>();

        CreateMap<ExampleDocument, ExampleCollectionDto>();
        CreateMap<ExampleDocument, ExampleDetailsDto>();
    }
}