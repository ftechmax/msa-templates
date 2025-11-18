using System.Linq.Expressions;
using ApplicationName.Worker.Application.Documents;

namespace ApplicationName.Worker.Application;

public interface IDocumentRepository
{
    Task<T> GetAsync<T>(Expression<Func<T, bool>> expr) where T : DocumentBase;

    Task<IEnumerable<T>> GetAllAsync<T>() where T : DocumentBase;

    Task UpsertAsync(ExampleDocument document);
}