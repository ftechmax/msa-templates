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
using AutoMapper;
using FakeItEasy;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using NUnit.Framework;

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

        var mappingConfig = new MapperConfiguration(cfg => { cfg.AddProfile(new MappingProfile()); });
        _mapper = mappingConfig.CreateMapper();
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

        result.Should()
            .NotBeNull()
            .And.HaveSameCount(dtos)
            .And.Contain(dtos);
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

        capturedDtos.Should()
            .NotBeNullOrEmpty()
            .And.HaveSameCount(documents)
            .And.BeEquivalentTo(expectedDtos);

        result.Should()
            .NotBeNullOrEmpty()
            .And.HaveSameCount(capturedDtos)
            .And.Contain(capturedDtos);
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

        result.Should().NotBeNull().And.BeEmpty();
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

        result.Should()
            .NotBeNull()
            .And.BeEquivalentTo(dto);
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

        result.Should()
            .NotBeNull()
            .And.BeOfType<ExampleDetailsDto>()
            .And.BeEquivalentTo(document, i => i.ExcludingMissingMembers());
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

        result.Should().BeNull();
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

        capturedCommand.Should().NotBeNull();
        capturedCommand.Name.Should().NotBeNullOrWhiteSpace().And.Be(dto.Name);
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

        capturedCommand.Should().NotBeNull();
        capturedCommand.CorrelationId.Should().NotBeEmpty().And.Be(dto.CorrelationId);
        capturedCommand.Id.Should().NotBeEmpty().And.Be(id);
        capturedCommand.Description.Should().NotBeNullOrWhiteSpace().And.Be(dto.Description);
        capturedCommand.ExampleValueObject.Should().NotBeNull().And.BeEquivalentTo(dto.ExampleValueObject);
    }

    public static ExampleDocument GenerateDocument(IExample source)
    {
        var ci = typeof(ExampleDocument).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, Type.EmptyTypes);
        var instance = (ExampleDocument)ci!.Invoke(null);

        var mapper = new MapperConfiguration(configure =>
        {
            configure.ShouldMapProperty = i => i.PropertyType.IsPublic || i.PropertyType.IsNotPublic;
            configure.CreateMap<IAggregate, DocumentBase>();
            configure.CreateMap<IExample, ExampleDocument>();
            configure.CreateMap<IExampleValueObject, ExampleValueObject>();
        }).CreateMapper();

        mapper.Map(A.Dummy<IAggregate>(), instance);

        return mapper.Map(source, instance);
    }
}