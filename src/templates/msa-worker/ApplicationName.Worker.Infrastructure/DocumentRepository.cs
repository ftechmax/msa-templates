using ApplicationName.Worker.Application;
using ApplicationName.Worker.Application.Documents;
using ApplicationName.Worker.Contracts;
using Dawn;
using MongoDB.Driver;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace ApplicationName.Worker.Infrastructure;

[ExcludeFromCodeCoverage]
public class DocumentRepository : IDocumentRepository
{
    private readonly IMongoDatabase _mongoDatabase;

    public DocumentRepository(IMongoClient mongoClient)
    {
        _mongoDatabase = mongoClient.GetDatabase(ApplicationConstants.DatabaseName);
    }

    public async Task<T> GetAsync<T>() where T : DocumentBase
    {
        var collection = GetCollection<T>();

        var cursor = await collection.FindAsync(_ => true);

        return await cursor.SingleOrDefaultAsync();
    }

    public Task UpsertAsync(ExampleDocument document)
    {
        Guard.Argument(document, nameof(document)).NotNull();
        Guard.Argument(document.Id, nameof(document.Id)).NotDefault();
        Guard.Argument(document.Created, nameof(document.Created)).NotDefault();
        Guard.Argument(document.Updated, nameof(document.Updated)).NotDefault();

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