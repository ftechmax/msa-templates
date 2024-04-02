using ApplicationName.Shared.Commands;
using ApplicationName.Worker.Application.Documents;
using ApplicationName.Worker.Application.DomainEvents;
using ApplicationName.Worker.Contracts.Commands;
using ArgDefender;

namespace ApplicationName.Worker.Application.Services;

public class ApplicationService(IDocumentRepository documentRepository) : IApplicationService
{
    public async Task<ExampleCreated> HandleAsync(ICreateExampleCommand command)
    {
        Guard.Argument(command).NotNull();

        var (aggregate, domainEvent) = ExampleDocument.Create(command);

        await documentRepository.UpsertAsync(aggregate);

        return domainEvent;
    }

    public async Task<ExampleUpdated> HandleAsync(IUpdateExampleCommand command)
    {
        Guard.Argument(command).NotNull();
        Guard.Argument(command.Id).NotDefault();

        var document = await documentRepository.GetAsync<ExampleDocument>(i => i.Id == command.Id);

        var domainEvent = document.Update(command);

        await documentRepository.UpsertAsync(document);

        return domainEvent;
    }

    public async Task<ExampleEntityAdded> HandleAsync(IAddExampleEntityCommand command)
    {
        Guard.Argument(command).NotNull();
        Guard.Argument(command.Id).NotDefault();

        var document = await documentRepository.GetAsync<ExampleDocument>(i => i.Id == command.Id);

        var domainEvent = document.AddExampleEntity(command);

        await documentRepository.UpsertAsync(document);

        return domainEvent;
    }

    public async Task<ExampleEntityUpdated> HandleAsync(IUpdateExampleEntityCommand command)
    {
        Guard.Argument(command).NotNull();
        Guard.Argument(command.Id).NotDefault();

        var document = await documentRepository.GetAsync<ExampleDocument>(i => i.Id == command.Id);

        var domainEvent = document.UpdateExampleEntity(command);

        await documentRepository.UpsertAsync(document);

        return domainEvent;
    }

    public async Task<ExampleRemoteCodeSet> HandleAsync(ISetExampleRemoteCodeCommand command)
    {
        Guard.Argument(command).NotNull();
        Guard.Argument(command.Id).NotDefault();

        var document = await documentRepository.GetAsync<ExampleDocument>(i => i.Id == command.Id);

        var domainEvent = document.SetRemoteCode(command);

        await documentRepository.UpsertAsync(document);

        return domainEvent;
    }
}