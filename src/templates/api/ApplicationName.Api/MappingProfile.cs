using System.Diagnostics.CodeAnalysis;
using ApplicationName.Api.Application.Documents;
using ApplicationName.Api.Contracts.Dtos;
using ApplicationName.Shared.Commands;
using ApplicationName.Shared.Events;
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
        CreateMap<ExampleDocument, ExampleCollectionDto>();
        CreateMap<ExampleDocument, ExampleDetailsDto>();
        CreateMap<ExampleValueObject, ExampleValueObjectDto>();
    }
}