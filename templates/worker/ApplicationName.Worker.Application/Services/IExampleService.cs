using ApplicationName.Shared.Commands;
using ApplicationName.Worker.Application.DomainEvents;
using ApplicationName.Worker.Contracts.Commands;

namespace ApplicationName.Worker.Application.Services;

public interface IExampleService
{
    Task<ExampleCreated> HandleAsync(CreateExampleCommand command);

    Task<ExampleUpdated> HandleAsync(UpdateExampleCommand command);

    Task<ExampleRemoteCodeSet> HandleAsync(SetExampleRemoteCodeCommand command);
}