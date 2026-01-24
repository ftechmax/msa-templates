using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using ApplicationName.Shared.Projections;
using ApplicationName.Worker.Application;
using ApplicationName.Worker.Application.Documents;
using ApplicationName.Worker.Contracts;
using ArgDefender;
using MapsterMapper;
using MongoDB.Driver;

namespace ApplicationName.Worker.Infrastructure;

[ExcludeFromCodeCoverage]
public class DocumentRepository(IMongoClient mongoClient, IMapper mapper, IProtoCacheRepository protoCacheRepository) : IDocumentRepository
{
    private readonly IMongoDatabase _mongoDatabase = mongoClient.GetDatabase(ApplicationConstants.DatabaseName);

    public async Task<T> GetAsync<T>(Expression<Func<T, bool>> expr) where T : DocumentBase
    {
        Guard.Argument(expr).NotNull();

        var collection = GetCollection<T>();

        var cursor = await collection.FindAsync(expr);

        return await cursor.SingleOrDefaultAsync();
    }

    public async Task<IEnumerable<T>> GetAllAsync<T>() where T : DocumentBase
    {
        var collection = GetCollection<T>();
        var cursor = await collection.FindAsync(FilterDefinition<T>.Empty);
        return await cursor.ToListAsync();
    }

    public async Task<IEnumerable<DocumentBase>> GetAllByTypeAsync(Type documentType)
    {
        Guard.Argument(documentType).NotNull();

        // Use reflection to call GetAllAsync<T> with the concrete type
        var getAllMethod = GetType()
            .GetMethod(nameof(GetAllAsync))!
            .MakeGenericMethod(documentType);

        var task = (Task)getAllMethod.Invoke(this, null)!;
        await task.ConfigureAwait(false);
        
        // Get the result from the completed task
        var resultProperty = task.GetType().GetProperty("Result")!;
        var result = (IEnumerable<DocumentBase>)resultProperty.GetValue(task)!;
        return result;
    }

    public async Task<DocumentBase?> GetByIdAndTypeAsync(Guid id, Type documentType)
    {
        Guard.Argument(id).NotDefault();
        Guard.Argument(documentType).NotNull();

        // Build expression: d => d.Id == id
        var parameter = Expression.Parameter(documentType, "d");
        var idProperty = Expression.Property(parameter, nameof(DocumentBase.Id));
        var idValue = Expression.Constant(id);
        var equality = Expression.Equal(idProperty, idValue);
        var lambda = Expression.Lambda(equality, parameter);

        // Use reflection to call GetAsync<T> with the concrete type
        var getMethod = GetType()
            .GetMethod(nameof(GetAsync))!
            .MakeGenericMethod(documentType);

        var task = (Task)getMethod.Invoke(this, [lambda])!;
        await task.ConfigureAwait(false);
        
        // Get the result from the completed task
        var resultProperty = task.GetType().GetProperty("Result")!;
        var result = (DocumentBase?)resultProperty.GetValue(task);
        return result;
    }

    public async Task UpsertAsync(ExampleDocument document)
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
            .SetOnInsert(i => i.Name, document.Name)
            .Set(i => i.Description, document.Description)
            .Set(i => i.ExampleValueObject, document.ExampleValueObject)
            .Set(i => i.RemoteCode, document.RemoteCode);

        await collection.UpdateOneAsync(i => i.Id == document.Id, updateDefinition, new UpdateOptions { IsUpsert = true });

        var projection = mapper.Map<ExampleProjection>(document);
        await protoCacheRepository.SetAsync(ApplicationConstants.ExampleProjectionByIdCacheKey(document.Id), projection);
    }

    private IMongoCollection<T> GetCollection<T>() where T : DocumentBase
    {
        return _mongoDatabase.GetCollection<T>($"{typeof(T).Name}s");
    }
}