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
        CreateMap<ExampleEntityAdded, ExampleEntityAddedEvent>();
        CreateMap<ExampleEntityUpdated, ExampleEntityUpdatedEvent>();
        CreateMap<ExampleRemoteCodeSet, ExampleRemoteCodeSetEvent>();
    }
}