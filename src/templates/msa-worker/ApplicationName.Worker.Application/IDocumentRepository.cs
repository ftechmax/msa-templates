using ApplicationName.Worker.Application.Documents;
using System.Threading.Tasks;

namespace ApplicationName.Worker.Application;

public interface IDocumentRepository
{
    Task<T> GetAsync<T>() where T : DocumentBase;

    Task UpsertAsync(ExampleDocument document);
}