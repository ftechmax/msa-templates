using ApplicationName.Shared.Projections;
using ApplicationName.Worker.Application.Documents;
using ApplicationName.Worker.Application.DomainEvents;
using ApplicationName.Worker.Commands;
using ApplicationName.Worker.Events;
using AutoMapper;
using Other.Worker.Contracts.Commands;

namespace ApplicationName.Worker;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<IExternalEvent, SetExampleRemoteCodeCommand>()
            .ForMember(dst => dst.RemoteCode, src =>
                src.MapFrom(i => i.Code));

        CreateMap<ExampleCreated, ExampleCreatedEvent>();
        CreateMap<ExampleUpdated, ExampleUpdatedEvent>();
        CreateMap<ExampleValueObjectEvent, ExampleValueObjectEventData>();
        CreateMap<ExampleEntityAdded, ExampleEntityAddedEvent>();
        CreateMap<ExampleEntityUpdated, ExampleEntityUpdatedEvent>();
        CreateMap<ExampleRemoteCodeSet, ExampleRemoteCodeSetEvent>();

        // Projections
        CreateMap<DocumentBase, ProjectionBase>();
        CreateMap<ExampleDocument, ExampleProjection>();
        CreateMap<ExampleEntity, ExampleEntityProjection>();
        CreateMap<ExampleValueObject, ExampleValueObjectProjection>();
    }
}