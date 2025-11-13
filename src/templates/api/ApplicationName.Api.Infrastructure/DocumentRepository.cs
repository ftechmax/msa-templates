using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using ApplicationName.Api.Application.Documents;
using ApplicationName.Api.Application.Repositories;
using ApplicationName.Api.Contracts;
using MongoDB.Driver;

namespace ApplicationName.Api.Infrastructure;

[ExcludeFromCodeCoverage]
public sealed class DocumentRepository(IMongoClient mongoClient) : IDocumentRepository
{
    private readonly IMongoDatabase _mongoDatabase = mongoClient.GetDatabase(ApplicationConstants.DatabaseName);

    public async Task<T?> GetAsync<T>(Expression<Func<T, bool>> expr) where T : DocumentBase
    {
        var collection = GetCollection<T>();

        var cursor = await collection.FindAsync(expr);

        return await cursor.SingleOrDefaultAsync();
    }

    public async Task<IEnumerable<T>> GetAllAsync<T>(Expression<Func<T, bool>> expr) where T : DocumentBase
    {
        var collection = GetCollection<T>();

        var cursor = await collection.FindAsync(expr);

        var documents = await cursor.ToListAsync();

        return documents;
    }

    private IMongoCollection<T> GetCollection<T>() where T : DocumentBase
    {
        return _mongoDatabase.GetCollection<T>($"{typeof(T).Name}s");
    }
}