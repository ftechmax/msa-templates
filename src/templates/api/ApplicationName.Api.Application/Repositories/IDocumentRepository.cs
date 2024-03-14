using System.Linq.Expressions;
using ApplicationName.Api.Application.Documents;

namespace ApplicationName.Api.Application.Repositories;

public interface IDocumentRepository
{
    Task<T> GetAsync<T>(Expression<Func<T, bool>> expr) where T : DocumentBase;

    Task<IEnumerable<T>> GetAllAsync<T>(Expression<Func<T, bool>> expr) where T : DocumentBase;
}