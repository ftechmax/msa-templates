using ApplicationName.Shared.Commands;
using ApplicationName.Shared.Events;
using ApplicationName.Shared.Projections;
using ApplicationName.Worker.Application.Documents;
using ApplicationName.Worker.Contracts;
using ApplicationName.Worker.Infrastructure;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using MapsterMapper;
using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;
using Shouldly;

namespace ApplicationName.Worker.Test.Infrastructure;

public class DocumentRepositoryTest
{
    private IFixture _fixture;

    private IMongoClient _mongoClient;

    private IMongoDatabase _mongoDatabase;

    private IMongoCollection<ExampleDocument> _collection;

    private IMapper _mapper;

    private IProtoCacheRepository _protoCacheRepository;

    private DocumentRepository _subjectUnderTest;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization());

        _mongoClient = _fixture.Freeze<IMongoClient>();
        _mongoDatabase = _fixture.Freeze<IMongoDatabase>();
        _collection = _fixture.Freeze<IMongoCollection<ExampleDocument>>();
        _mapper = _fixture.Freeze<IMapper>();
        _protoCacheRepository = _fixture.Freeze<IProtoCacheRepository>();

        A.CallTo(() => _mongoClient.GetDatabase(ApplicationConstants.DatabaseName, A<MongoDatabaseSettings>._)).Returns(_mongoDatabase);
        A.CallTo(() => _mongoDatabase.GetCollection<ExampleDocument>(A<string>._, A<MongoCollectionSettings>._)).Returns(_collection);
        A.CallTo(() => _collection.UpdateOneAsync(A<FilterDefinition<ExampleDocument>>._, A<UpdateDefinition<ExampleDocument>>._, A<UpdateOptions>._, A<CancellationToken>._))
            .Returns(Task.FromResult<UpdateResult>(new UpdateResult.Acknowledged(1, 1, BsonNull.Value)));

        _subjectUnderTest = _fixture.Create<DocumentRepository>();
    }

    [Test]
    public async Task UpsertAsync_Sets_Projection_Cache_Key()
    {
        // Arrange
        var command = new CreateExampleCommand
        {
            CorrelationId = Guid.NewGuid(),
            Name = "name",
            Description = "description",
            ExampleValueObject = new ExampleValueObjectEventData { Code = "code", Value = 1.23 }
        };

        var document = new ExampleDocument(command);
        var projection = new ExampleProjection
        {
            Id = document.Id,
            Created = document.Created,
            Updated = document.Updated,
            Name = document.Name,
            Description = document.Description,
            ExampleValueObject = new ExampleValueObjectProjection
            {
                Code = document.ExampleValueObject.Code,
                Value = document.ExampleValueObject.Value
            },
            RemoteCode = document.RemoteCode
        };

        A.CallTo(() => _mapper.Map<ExampleProjection>(A<ExampleDocument>._)).Returns(projection);

        // Act
        await _subjectUnderTest.UpsertAsync(document);

        // Assert
        A.CallTo(() => _protoCacheRepository.SetAsync(ApplicationConstants.ExampleProjectionByIdCacheKey(document.Id), projection, A<Microsoft.Extensions.Caching.Distributed.DistributedCacheEntryOptions>._))
            .MustHaveHappenedOnceExactly();
    }
}
