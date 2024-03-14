using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using ApplicationName.Worker.Application;
using ApplicationName.Worker.Application.Documents;
using ApplicationName.Worker.Contracts;
using ArgDefender;
using MongoDB.Driver;

namespace ApplicationName.Worker.Infrastructure;

[ExcludeFromCodeCoverage]
public class DocumentRepository(IMongoClient mongoClient) : IDocumentRepository
{
    private readonly IMongoDatabase _mongoDatabase = mongoClient.GetDatabase(ApplicationConstants.DatabaseName);

    public async Task<T> GetAsync<T>(Expression<Func<T, bool>> expr) where T : DocumentBase
    {
        Guard.Argument(expr).NotNull();

        var collection = GetCollection<T>();

        var cursor = await collection.FindAsync(expr);

        return await cursor.SingleOrDefaultAsync();
    }

    public Task UpsertAsync(ExampleDocument document)
    {
        Guard.Argument(document).NotNull();
        Guard.Argument(document.Id).NotDefault();
        Guard.Argument(document.Created).NotDefault();
        Guard.Argument(document.Updated).NotDefault();

        var collection = GetCollection<ExampleDocument>();
        var updateDefinition = Builders<ExampleDocument>.Update
            .SetOnInsert(i => i.Id, document.Id)
            .SetOnInsert(i => i.Created, document.Created)
            .Set(i => i.Updated, document.Updated)
            .Set(i => i.Name, document.Name);

        return collection.UpdateOneAsync(i => i.Id == document.Id, updateDefinition, new UpdateOptions { IsUpsert = true });
    }

    private IMongoCollection<T> GetCollection<T>() where T : DocumentBase
    {
        return _mongoDatabase.GetCollection<T>($"{typeof(T).Name}s");
    }
}