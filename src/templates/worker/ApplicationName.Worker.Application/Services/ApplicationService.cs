using ApplicationName.Shared.Commands;
using ApplicationName.Worker.Application.Documents;
using ApplicationName.Worker.Application.DomainEvents;
using ApplicationName.Worker.Contracts.Commands;
using ArgDefender;

namespace ApplicationName.Worker.Application.Services;

public class ApplicationService : IApplicationService
{
    private readonly IDocumentRepository _documentRepository;

    public ApplicationService(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<ExampleCreated> HandleAsync(ICreateExampleCommand command)
    {
        Guard.Argument(command).NotNull();

        var (aggregate, domainEvent) = ExampleDocument.Create(command);

        await _documentRepository.UpsertAsync(aggregate);

        return domainEvent;
    }

    public async Task<ExampleUpdated> HandleAsync(IUpdateExampleCommand command)
    {
        Guard.Argument(command).NotNull();
        Guard.Argument(command.Id).NotDefault();

        var document = await _documentRepository.GetAsync<ExampleDocument>(i => i.Id == command.Id);

        var domainEvent = document.Update(command);

        await _documentRepository.UpsertAsync(document);

        return domainEvent;
    }

    public async Task<ExampleEntityAdded> HandleAsync(IAddExampleEntityCommand command)
    {
        Guard.Argument(command).NotNull();
        Guard.Argument(command.Id).NotDefault();

        var document = await _documentRepository.GetAsync<ExampleDocument>(i => i.Id == command.Id);

        var domainEvent = document.AddExampleEntity(command);

        await _documentRepository.UpsertAsync(document);

        return domainEvent;
    }

    public async Task<ExampleEntityUpdated> HandleAsync(IUpdateExampleEntityCommand command)
    {
        Guard.Argument(command).NotNull();
        Guard.Argument(command.Id).NotDefault();

        var document = await _documentRepository.GetAsync<ExampleDocument>(i => i.Id == command.Id);

        var domainEvent = document.UpdateExampleEntity(command);

        await _documentRepository.UpsertAsync(document);

        return domainEvent;
    }

    public async Task<ExampleRemoteCodeSet> HandleAsync(ISetExampleRemoteCodeCommand command)
    {
        Guard.Argument(command).NotNull();
        Guard.Argument(command.Id).NotDefault();

        var document = await _documentRepository.GetAsync<ExampleDocument>(i => i.Id == command.Id);

        var domainEvent = document.SetRemoteCode(command);

        await _documentRepository.UpsertAsync(document);

        return domainEvent;
    }
}