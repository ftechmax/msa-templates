using System.Diagnostics.CodeAnalysis;
using ApplicationName.Api.Application.Documents;
using ApplicationName.Api.Contracts.Dtos;
using ApplicationName.Shared.Commands;
using ApplicationName.Shared.Events;
using Mapster;

namespace ApplicationName.Api;

[ExcludeFromCodeCoverage]
public class MappingProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // API -> Application
        config.NewConfig<CreateExampleDto, CreateExampleCommand>();
        config.NewConfig<UpdateExampleDto, UpdateExampleCommand>();
        config.NewConfig<ExampleValueObjectDto, ExampleValueObjectEventData>();

        // Application -> API
        config.NewConfig<ExampleDocument, ExampleCollectionDto>();
        config.NewConfig<ExampleDocument, ExampleDetailsDto>();
        config.NewConfig<ExampleValueObject, ExampleValueObjectDto>();
    }
}