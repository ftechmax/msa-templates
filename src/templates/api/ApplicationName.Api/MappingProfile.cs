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
        // API -> Application
        CreateMap<CreateExampleDto, CreateExampleCommand>();
        CreateMap<UpdateExampleDto, UpdateExampleCommand>();
        CreateMap<AddExampleEntityDto, AddExampleEntityCommand>();
        CreateMap<UpdateExampleEntityDto, UpdateExampleEntityCommand>();
        CreateMap<ExampleValueObjectDto, ExampleValueObjectEventData>();

        // Application -> API
        CreateMap<ExampleDocument, ExampleCollectionDto>();
        CreateMap<ExampleDocument, ExampleDetailsDto>();
        CreateMap<ExampleEntity, ExampleEntityDto>();
        CreateMap<ExampleValueObject, ExampleValueObjectDto>();
    }
}