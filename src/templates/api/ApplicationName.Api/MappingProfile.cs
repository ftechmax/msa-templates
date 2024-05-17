using System.Diagnostics.CodeAnalysis;
using ApplicationName.Api.Application.Commands;
using ApplicationName.Api.Contracts.Dtos;
using ApplicationName.Shared.Projections;
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
        CreateMap<ExampleProjection, ExampleCollectionDto>();
        CreateMap<ExampleProjection, ExampleDetailsDto>();
        CreateMap<ExampleEntityProjection, ExampleEntityDto>();
        CreateMap<ExampleValueObjectProjection, ExampleValueObjectDto>();
    }
}