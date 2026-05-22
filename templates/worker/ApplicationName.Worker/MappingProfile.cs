using System.Diagnostics.CodeAnalysis;
using ApplicationName.Shared.Events;
using ApplicationName.Shared.Projections;
using ApplicationName.Worker.Application.Documents;
using ApplicationName.Worker.Application.DomainEvents;
using ApplicationName.Worker.Contracts.Commands;
using Mapster;
using Other.Worker.Contracts.Commands;

namespace ApplicationName.Worker;

[ExcludeFromCodeCoverage]
public class MappingProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ExternalEvent, SetExampleRemoteCodeCommand>().Map(dest => dest.RemoteCode, src => src.Code);
        config.NewConfig<ExampleCreated, ExampleCreatedEvent>();
        config.NewConfig<ExampleUpdated, ExampleUpdatedEvent>();
        config.NewConfig<ExampleValueObjectEvent, ExampleValueObjectEventData>();
        config.NewConfig<ExampleRemoteCodeSet, ExampleRemoteCodeSetEvent>();

        // Projections
        config.NewConfig<DocumentBase, ProjectionBase>();
        config.NewConfig<ExampleDocument, ExampleProjection>();
        config.NewConfig<ExampleValueObject, ExampleValueObjectProjection>();
    }
}