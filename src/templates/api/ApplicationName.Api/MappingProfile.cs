using System.Diagnostics.CodeAnalysis;
using ApplicationName.Api.Contracts.Dtos;
using ApplicationName.Shared.Commands;
using ApplicationName.Shared.Events;
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
        CreateMap<ExampleValueObjectDto, ExampleValueObjectEventData>();

        // Application -> API
        CreateMap<ExampleProjection, ExampleCollectionDto>();
        CreateMap<ExampleProjection, ExampleDetailsDto>();
        CreateMap<ExampleValueObjectProjection, ExampleValueObjectDto>();
    }
}