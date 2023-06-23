using ApplicationName.Worker.Application.Documents;
using ApplicationName.Worker.Commands;
using ApplicationName.Worker.Events;
using AutoMapper;
using Other.Worker.Contracts.Commands;

namespace ApplicationName.Worker;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<IExternalEvent, ExampleCommand>();
        CreateMap<ExampleDocument, ExampleCreatedEvent>();
        CreateMap<ExampleDocument, ExampleUpdatedEvent>();
    }
}