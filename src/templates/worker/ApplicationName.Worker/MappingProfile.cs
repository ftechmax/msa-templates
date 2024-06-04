using ApplicationName.Shared.Events;
using ApplicationName.Worker.Application.DomainEvents;
using ApplicationName.Worker.Contracts.Commands;
using AutoMapper;
using Other.Worker.Contracts.Commands;

namespace ApplicationName.Worker;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<ExternalEvent, SetExampleRemoteCodeCommand>()
            .ForMember(dst => dst.RemoteCode, src =>
                src.MapFrom(i => i.Code));

        CreateMap<ExampleCreated, ExampleCreatedEvent>();
        CreateMap<ExampleUpdated, ExampleUpdatedEvent>();
        CreateMap<ExampleValueObjectEvent, ExampleValueObjectEventData>();
        CreateMap<ExampleRemoteCodeSet, ExampleRemoteCodeSetEvent>();
    }
}