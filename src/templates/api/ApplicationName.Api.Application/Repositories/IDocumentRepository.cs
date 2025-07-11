using ApplicationName.Api.Application.Documents;
using System.Linq.Expressions;

namespace ApplicationName.Api.Application.Repositories;

public interface IDocumentRepository
{
    Task<T?> GetAsync<T>(Expression<Func<T, bool>> expr) where T : DocumentBase;

    Task<IEnumerable<T>> GetAllAsync<T>(Expression<Func<T, bool>> expr) where T : DocumentBase;
}