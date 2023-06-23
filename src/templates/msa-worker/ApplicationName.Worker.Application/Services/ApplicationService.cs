using ApplicationName.Worker.Application.Documents;
using ApplicationName.Worker.Contracts.Commands;
using Dawn;
using System.Threading.Tasks;

namespace ApplicationName.Worker.Application.Services;

public class ApplicationService : IApplicationService
{
    private readonly IDocumentRepository _documentRepository;

    public ApplicationService(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<ExampleDocument> HandleAsync(IExampleCommand command)
    {
        Guard.Argument(command, nameof(command)).NotNull();

        var document = await _documentRepository.GetAsync<ExampleDocument>();
        if (document == default)
        {
            document = new ExampleDocument(command);
        }
        else
        {
            document.Update(command);
        }

        await _documentRepository.UpsertAsync(document);

        return document;
    }
}