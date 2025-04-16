using System.Linq.Expressions;
using System.Reflection;
using ApplicationName.Api.Application.Documents;
using ApplicationName.Api.Application.Repositories;
using ApplicationName.Api.Application.Services;
using ApplicationName.Api.Contracts;
using ApplicationName.Api.Contracts.Dtos;
using ApplicationName.Shared.Aggregates;
using ApplicationName.Shared.Commands;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using Mapster;
using MapsterMapper;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using NUnit.Framework;
using Shouldly;

namespace ApplicationName.Api.Application.Test.Services;

public class ExampleServiceTest
{
    private IFixture _fixture;

    private IDocumentRepository _documentRepository;

    private IProtoCacheRepository _protoCacheRepository;

    private IMapper _mapper;

    private IBus _bus;

    private ISendEndpoint _sendEndpoint;

    private ExampleService _subjectUnderTest;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization());

        _documentRepository = _fixture.Freeze<IDocumentRepository>();
        _protoCacheRepository = _fixture.Freeze<IProtoCacheRepository>();

        var queue = new Uri("queue:example");
        EndpointConvention.Map<CreateExampleCommand>(queue);
        EndpointConvention.Map<UpdateExampleCommand>(queue);

        _bus = _fixture.Freeze<IBus>();
        _sendEndpoint = _fixture.Freeze<ISendEndpoint>();
        A.CallTo(() => _bus.GetSendEndpoint(A<Uri>._)).Returns(_sendEndpoint);

        var mapperConfig = TypeAdapterConfig.GlobalSettings;
        mapperConfig.Scan(typeof(MappingProfile).Assembly);
        _mapper = new Mapper(mapperConfig);
        _fixture.Register(() => _mapper);

        _subjectUnderTest = _fixture.Create<ExampleService>();
    }

    [Test]
    public async Task GetCollectionAsync_From_Cache()
    {
        // Arrange
        var dtos = _fixture.CreateMany<ExampleCollectionDto>(3);

        A.CallTo(() => _protoCacheRepository.GetAsync<IEnumerable<ExampleCollectionDto>>(ApplicationConstants.ExampleCollectionCacheKey)).ReturnsLazily(() => dtos);

        // Act
        var result = await _subjectUnderTest.GetCollectionAsync();

        // Assert
        A.CallTo(() => _protoCacheRepository.GetAsync<IEnumerable<ExampleCollectionDto>>(ApplicationConstants.ExampleCollectionCacheKey)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _documentRepository.GetAllAsync(A<Expression<Func<ExampleDocument, bool>>>._)).MustNotHaveHappened();

        result.ShouldSatisfyAllConditions(
            i => i.ShouldNotBeNull(),
            i => i.ShouldNotBeEmpty(),
            i => i.Count().ShouldBe(dtos.Count()),
            i => i.ShouldBeSameAs(dtos));
    }

    [Test]
    public async Task GetCollectionAsync_From_Repository()
    {
        // Arrange
        var document1 = GenerateDocument(A.Dummy<IExample>());
        var document2 = GenerateDocument(A.Dummy<IExample>());
        var document3 = GenerateDocument(A.Dummy<IExample>());
        var documents = new[] { document1, document2, document3 };

        A.CallTo(() => _protoCacheRepository.GetAsync<IEnumerable<ExampleCollectionDto>>(ApplicationConstants.ExampleCollectionCacheKey))
            .ReturnsLazily(() => default(IEnumerable<ExampleCollectionDto>));
        A.CallTo(() => _documentRepository.GetAllAsync(A<Expression<Func<ExampleDocument, bool>>>._))
            .ReturnsLazily(() => documents);

        var capturedDtos = default(IEnumerable<ExampleCollectionDto>);
        A.CallTo(() => _protoCacheRepository.SetAsync(ApplicationConstants.ExampleCollectionCacheKey, A<IEnumerable<ExampleCollectionDto>>._, A<DistributedCacheEntryOptions>._)).Invokes(
            (string _, IEnumerable<ExampleCollectionDto> arg1, DistributedCacheEntryOptions _) =>
            {
                capturedDtos = arg1;
            });

        var expectedDtos = _mapper.Map<IEnumerable<ExampleCollectionDto>>(documents);

        // Act
        var result = await _subjectUnderTest.GetCollectionAsync();

        // Assert
        A.CallTo(() => _protoCacheRepository.GetAsync<IEnumerable<ExampleCollectionDto>>(ApplicationConstants.ExampleCollectionCacheKey)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _documentRepository.GetAllAsync(A<Expression<Func<ExampleDocument, bool>>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _protoCacheRepository.SetAsync(ApplicationConstants.ExampleCollectionCacheKey, A<IEnumerable<ExampleCollectionDto>>._, A<DistributedCacheEntryOptions>._)).MustHaveHappenedOnceExactly();

        capturedDtos.ShouldSatisfyAllConditions(
            i => i.ShouldNotBeNull(),
            i => i.ShouldNotBeEmpty(),
            i => i.Count().ShouldBe(documents.Length),
            i => i.ShouldBe(expectedDtos));

        result.ShouldSatisfyAllConditions(
            i => i.ShouldNotBeNull(),
            i => i.ShouldNotBeEmpty(),
            i => i.Count().ShouldBe(documents.Length),
            i => i.ShouldBe(expectedDtos));
    }

    [Test]
    public async Task GetCollectionAsync_From_Repository_Without_Document()
    {
        // Arrange
        A.CallTo(() => _protoCacheRepository.GetAsync<IEnumerable<ExampleCollectionDto>>(ApplicationConstants.ExampleCollectionCacheKey)).ReturnsLazily(() => default(IEnumerable<ExampleCollectionDto>));
        A.CallTo(() => _documentRepository.GetAllAsync(A<Expression<Func<ExampleDocument, bool>>>._)).Returns([]);

        // Act
        var result = await _subjectUnderTest.GetCollectionAsync();

        // Assert
        A.CallTo(() => _protoCacheRepository.SetAsync(A<string>._, A<IEnumerable<ExampleCollectionDto>>._, A<DistributedCacheEntryOptions>._)).MustNotHaveHappened();
        A.CallTo(() => _protoCacheRepository.GetAsync<IEnumerable<ExampleCollectionDto>>(A<string>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _documentRepository.GetAllAsync(A<Expression<Func<ExampleDocument, bool>>>._)).MustHaveHappenedOnceExactly();

        result.ShouldSatisfyAllConditions(
            i => i.ShouldNotBeNull(),
            i => i.ShouldBeEmpty());
    }

    [Test]
    public async Task GetAsync_From_Cache()
    {
        // Arrange
        var id = _fixture.Create<Guid>();
        var dto = _fixture.Create<ExampleDetailsDto>();
        var cacheKey = ApplicationConstants.ExampleDetailsCacheKey(id);

        A.CallTo(() => _protoCacheRepository.GetAsync<ExampleDetailsDto>(cacheKey)).ReturnsLazily(() => dto);

        // Act
        var result = await _subjectUnderTest.GetAsync(id);

        // Assert
        A.CallTo(() => _protoCacheRepository.GetAsync<ExampleDetailsDto>(cacheKey)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _documentRepository.GetAsync(A<Expression<Func<ExampleDocument, bool>>>._)).MustNotHaveHappened();

        result.ShouldSatisfyAllConditions(
            i => i.ShouldNotBeNull(),
            i => i.ShouldBeSameAs(dto));
    }

    [Test]
    public async Task GetAsync_From_Repository()
    {
        // Arrange
        var document = GenerateDocument(A.Dummy<IExample>());
        var cacheKey = ApplicationConstants.ExampleDetailsCacheKey(document.Id);

        A.CallTo(() => _documentRepository.GetAsync(A<Expression<Func<ExampleDocument, bool>>>._)).ReturnsLazily(() => document);
        A.CallTo(() => _protoCacheRepository.GetAsync<ExampleDetailsDto>(cacheKey)).Returns(default(ExampleDetailsDto));

        // Act
        var result = await _subjectUnderTest.GetAsync(document.Id);

        // Assert
        A.CallTo(() => _protoCacheRepository.SetAsync(A<string>._, A<ExampleDetailsDto>._, A<DistributedCacheEntryOptions>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _protoCacheRepository.GetAsync<ExampleDetailsDto>(cacheKey)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _documentRepository.GetAsync(A<Expression<Func<ExampleDocument, bool>>>._)).MustHaveHappenedOnceExactly();

        result.ShouldSatisfyAllConditions(
            i => i.ShouldNotBeNull(),
            i => i.ShouldBeOfType<ExampleDetailsDto>(),
            i => i.Id.ShouldBe(document.Id),
            i => i.Name.ShouldBe(document.Name),
            i => i.Description.ShouldBe(document.Description),
            i => i.ExampleValueObject.ShouldSatisfyAllConditions(
                j => j.ShouldNotBeNull(),
                j => j.Code.ShouldBe(document.ExampleValueObject.Code),
                j => j.Value.ShouldBe(document.ExampleValueObject.Value)),
            i => i.RemoteCode.ShouldBe(document.RemoteCode));
    }

    [Test]
    public async Task GetAsync_Without_Document()
    {
        // Arrange
        var id = _fixture.Create<Guid>();

        A.CallTo(() => _documentRepository.GetAsync(A<Expression<Func<ExampleDocument, bool>>>._)).ReturnsLazily(() => default(ExampleDocument));
        A.CallTo(() => _protoCacheRepository.GetAsync<ExampleDetailsDto>(A<string>._)).Returns(default(ExampleDetailsDto));

        // Act
        var result = await _subjectUnderTest.GetAsync(id);

        // Assert
        A.CallTo(() => _protoCacheRepository.SetAsync(A<string>._, A<ExampleDetailsDto>._, A<DistributedCacheEntryOptions>._)).MustNotHaveHappened();
        A.CallTo(() => _protoCacheRepository.GetAsync<ExampleDetailsDto>(A<string>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _documentRepository.GetAsync(A<Expression<Func<ExampleDocument, bool>>>._)).MustHaveHappenedOnceExactly();

        result.ShouldBeNull();
    }

    [Test]
    public async Task HandleAsync_CreateExampleDto()
    {
        // Arrange
        var dto = _fixture.Create<CreateExampleDto>();

        var capturedCommand = default(CreateExampleCommand);
        A.CallTo(() => _sendEndpoint.Send(A<CreateExampleCommand>._, A<CancellationToken>._)).Invokes(
            (CreateExampleCommand arg0, CancellationToken _) =>
            {
                capturedCommand = arg0;
            });

        // Act
        await _subjectUnderTest.HandleAsync(dto);

        // Assert
        A.CallTo(() => _sendEndpoint.Send(A<CreateExampleCommand>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();

        capturedCommand.ShouldNotBeNull();
        capturedCommand.Name.ShouldSatisfyAllConditions(
            i => i.ShouldNotBeNullOrWhiteSpace(),
            i => i.ShouldBe(dto.Name));
    }

    [Test]
    public async Task HandleAsync_UpdateExampleDto()
    {
        // Arrange
        var id = _fixture.Create<Guid>();
        var dto = _fixture.Create<UpdateExampleDto>();

        var capturedCommand = default(UpdateExampleCommand);
        A.CallTo(() => _sendEndpoint.Send(A<UpdateExampleCommand>._, A<CancellationToken>._)).Invokes(
            (UpdateExampleCommand arg0, CancellationToken _) =>
            {
                capturedCommand = arg0;
            });

        // Act
        await _subjectUnderTest.HandleAsync(id, dto);

        // Assert
        A.CallTo(() => _sendEndpoint.Send(A<UpdateExampleCommand>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();

        capturedCommand.ShouldNotBeNull();
        capturedCommand.CorrelationId.ShouldSatisfyAllConditions(
            i => i.ShouldNotBe(Guid.Empty),
            i => i.ShouldBe(dto.CorrelationId));
        capturedCommand.Id.ShouldSatisfyAllConditions(
            i => i.ShouldNotBe(Guid.Empty),
            i => i.ShouldBe(id));
        capturedCommand.Description.ShouldSatisfyAllConditions(
            i => i.ShouldNotBeNullOrWhiteSpace(),
            i => i.ShouldBe(dto.Description));
        capturedCommand.ExampleValueObject.ShouldSatisfyAllConditions(
            i => i.ShouldNotBeNull(),
            i => i.Code.ShouldBe(dto.ExampleValueObject.Code),
            i => i.Value.ShouldBe(dto.ExampleValueObject.Value));
    }

    public static ExampleDocument GenerateDocument(IExample source)
    {
        var ci = typeof(ExampleDocument).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, Type.EmptyTypes);
        var instance = (ExampleDocument)ci!.Invoke(null);

        var config = TypeAdapterConfig.GlobalSettings;
        config.NewConfig<IAggregate, DocumentBase>();
        config.NewConfig<IExample, ExampleDocument>();
        config.NewConfig<IExampleValueObject, ExampleValueObject>()
            .MapToConstructor(typeof(ExampleValueObject).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, Type.EmptyTypes)!);
        var mapper = new Mapper(config);

        mapper.Map(A.Dummy<IAggregate>(), instance);

        return mapper.Map(source, instance);
    }
}