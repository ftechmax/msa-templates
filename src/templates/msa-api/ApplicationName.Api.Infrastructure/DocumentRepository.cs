using ApplicationName.Api.Application.Documents;
using ApplicationName.Api.Application.Repositories;
using ApplicationName.Api.Contracts;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ApplicationName.Api.Infrastructure;

[ExcludeFromCodeCoverage]
public sealed class DocumentRepository : IDocumentRepository
{
    private readonly IMongoDatabase _mongoDatabase;

    public DocumentRepository(IMongoClient mongoClient)
    {
        _mongoDatabase = mongoClient.GetDatabase(ApplicationConstants.DatabaseName);
    }

    public async Task<T> GetAsync<T>(Guid id) where T : DocumentBase
    {
        var collection = GetCollection<T>();

        var cursor = await collection.FindAsync(i => i.Id == id);

        return await cursor.SingleOrDefaultAsync();
    }

    public async Task<IEnumerable<T>> FindAsync<T>(Expression<Func<T, bool>> expr) where T : DocumentBase
    {
        var collection = GetCollection<T>();

        var cursor = await collection.FindAsync(expr);

        return await cursor.ToListAsync();
    }

    private IMongoCollection<T> GetCollection<T>() where T : DocumentBase
    {
        return _mongoDatabase.GetCollection<T>($"{typeof(T).Name}s");
    }
}