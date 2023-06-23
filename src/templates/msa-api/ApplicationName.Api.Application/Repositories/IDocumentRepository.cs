using ApplicationName.Api.Application.Documents;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ApplicationName.Api.Application.Repositories;

public interface IDocumentRepository
{
    Task<T> GetAsync<T>(Guid id) where T : DocumentBase;

    Task<IEnumerable<T>> FindAsync<T>(Expression<Func<T, bool>> expr) where T : DocumentBase;
}