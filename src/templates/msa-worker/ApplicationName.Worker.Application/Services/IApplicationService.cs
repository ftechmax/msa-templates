using System.Threading.Tasks;
using ApplicationName.Worker.Application.DomainEvents;
using ApplicationName.Worker.Contracts.Commands;

namespace ApplicationName.Worker.Application.Services;

public interface IApplicationService
{
    Task<ExampleCreated> HandleAsync(ICreateExampleCommand command);

    Task<ExampleUpdated> HandleAsync(IUpdateExampleCommand command);

    Task<ExampleEntityAdded> HandleAsync(IAddExampleEntityCommand command);

    Task<ExampleEntityUpdated> HandleAsync(IUpdateExampleEntityCommand command);

    Task<ExampleRemoteCodeSet> HandleAsync(ISetExampleRemoteCodeCommand command);
}