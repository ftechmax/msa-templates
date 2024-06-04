using ApplicationName.Shared.Commands;
using ApplicationName.Worker.Application.Documents;
using ApplicationName.Worker.Application.DomainEvents;
using ApplicationName.Worker.Contracts.Commands;
using ArgDefender;

namespace ApplicationName.Worker.Application.Services;

public class ApplicationService(IDocumentRepository documentRepository) : IApplicationService
{
    public async Task<ExampleCreated> HandleAsync(CreateExampleCommand command)
    {
        Guard.Argument(command, nameof(command)).NotNull();

        var (aggregate, domainEvent) = ExampleDocument.Create(command);

        await documentRepository.UpsertAsync(aggregate);

        return domainEvent;
    }

    public async Task<ExampleUpdated> HandleAsync(UpdateExampleCommand command)
    {
        Guard.Argument(command, nameof(command)).NotNull();
        Guard.Argument(command.Id, nameof(command.Id)).NotDefault();

        var document = await documentRepository.GetAsync<ExampleDocument>(i => i.Id == command.Id);

        var domainEvent = document.Update(command);

        await documentRepository.UpsertAsync(document);

        return domainEvent;
    }

    public async Task<ExampleRemoteCodeSet> HandleAsync(SetExampleRemoteCodeCommand command)
    {
        Guard.Argument(command, nameof(command)).NotNull();
        Guard.Argument(command.Id, nameof(command.Id)).NotDefault();

        var document = await documentRepository.GetAsync<ExampleDocument>(i => i.Id == command.Id);

        var domainEvent = document.SetRemoteCode(command);

        await documentRepository.UpsertAsync(document);

        return domainEvent;
    }
}