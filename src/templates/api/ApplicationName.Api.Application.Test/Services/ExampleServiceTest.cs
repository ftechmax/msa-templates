using ApplicationName.Api.Application.Repositories;
using ApplicationName.Api.Application.Services;
using ApplicationName.Api.Contracts;
using ApplicationName.Api.Contracts.Dtos;
using ApplicationName.Shared.Commands;
using ApplicationName.Shared.Projections;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using AutoMapper;
using FakeItEasy;
using FluentAssertions;
using MassTransit;
using NUnit.Framework;

namespace ApplicationName.Api.Application.Test.Services;

public class ExampleServiceTest
{
    private IFixture _fixture;

    private IProtoCacheRepository _protoCacheRepository;

    private IMapper _mapper;

    private IBus _bus;

    private ISendEndpoint _sendEndpoint;

    private ExampleService _subjectUnderTest;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization());

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
        A.CallTo(() => _protoCacheRepository.GetAllAsync<ExampleProjection>(ApplicationConstants.ExampleProjectionCacheNamespace)).MustNotHaveHappened();

        result.Should()
            .NotBeNull()
            .And.HaveSameCount(dtos)
            .And.Contain(dtos);
    }

    [Test]
    public async Task GetCollectionAsync_From_Projection()
    {
        // Arrange
        var projections = _fixture.CreateMany<ExampleProjection>();

        A.CallTo(() => _protoCacheRepository.GetAsync<IEnumerable<ExampleCollectionDto>>(ApplicationConstants.ExampleCollectionCacheKey))
            .ReturnsLazily(() => default(IEnumerable<ExampleCollectionDto>));
        A.CallTo(() => _protoCacheRepository.GetAllAsync<ExampleProjection>(ApplicationConstants.ExampleProjectionCacheNamespace))
            .ReturnsLazily(() => projections);

        var capturedDtos = default(IEnumerable<ExampleCollectionDto>);
        A.CallTo(() => _protoCacheRepository.SetAsync(ApplicationConstants.ExampleCollectionCacheKey, A<IEnumerable<ExampleCollectionDto>>._, A<TimeSpan?>._)).Invokes(
            (string _, IEnumerable<ExampleCollectionDto> arg1, TimeSpan? _) =>
            {
                capturedDtos = arg1;
            });

        var expectedDtos = _mapper.Map<IEnumerable<ExampleCollectionDto>>(projections);

        // Act
        var result = await _subjectUnderTest.GetCollectionAsync();

        // Assert
        A.CallTo(() => _protoCacheRepository.GetAsync<IEnumerable<ExampleCollectionDto>>(ApplicationConstants.ExampleCollectionCacheKey)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _protoCacheRepository.GetAllAsync<ExampleProjection>(ApplicationConstants.ExampleProjectionCacheNamespace)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _protoCacheRepository.SetAsync(ApplicationConstants.ExampleCollectionCacheKey, A<IEnumerable<ExampleCollectionDto>>._, A<TimeSpan?>._)).MustHaveHappenedOnceExactly();

        capturedDtos.Should()
            .NotBeNullOrEmpty()
            .And.HaveSameCount(projections)
            .And.BeEquivalentTo(expectedDtos);

        result.Should()
            .NotBeNullOrEmpty()
            .And.HaveSameCount(capturedDtos)
            .And.Contain(capturedDtos);
    }

    [Test]
    public async Task GetCollectionAsync_From_Repository_Without_Projections()
    {
        // Arrange
        A.CallTo(() => _protoCacheRepository.GetAsync<IEnumerable<ExampleCollectionDto>>(ApplicationConstants.ExampleCollectionCacheKey)).ReturnsLazily(() => default(IEnumerable<ExampleCollectionDto>));
        A.CallTo(() => _protoCacheRepository.GetAllAsync<ExampleProjection>(ApplicationConstants.ExampleProjectionCacheNamespace)).Returns([]);

        // Act
        var result = await _subjectUnderTest.GetCollectionAsync();

        // Assert
        A.CallTo(() => _protoCacheRepository.SetAsync(A<string>._, A<IEnumerable<ExampleCollectionDto>>._, A<TimeSpan?>._)).MustNotHaveHappened();
        A.CallTo(() => _protoCacheRepository.GetAsync<IEnumerable<ExampleCollectionDto>>(A<string>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _protoCacheRepository.GetAllAsync<ExampleProjection>(ApplicationConstants.ExampleProjectionCacheNamespace)).MustHaveHappenedOnceExactly();

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
        A.CallTo(() => _protoCacheRepository.GetAsync<ExampleProjection>(ApplicationConstants.ExampleProjectionCacheNamespace)).MustNotHaveHappened();

        result.Should()
            .NotBeNull()
            .And.BeEquivalentTo(dto);
    }

    [Test]
    public async Task GetAsync_From_Repository()
    {
        // Arrange
        var id = _fixture.Create<Guid>();
        var projection = _fixture.Create<ExampleProjection>();
        var cacheKey = ApplicationConstants.ExampleDetailsCacheKey(id);
        var projectionCacheKey = ApplicationConstants.ExampleProjectionCacheKey(id);

        A.CallTo(() => _protoCacheRepository.GetAsync<ExampleProjection>(projectionCacheKey)).ReturnsLazily(() => projection);
        A.CallTo(() => _protoCacheRepository.GetAsync<ExampleDetailsDto>(cacheKey)).Returns(default(ExampleDetailsDto));

        // Act
        var result = await _subjectUnderTest.GetAsync(id);

        // Assert
        A.CallTo(() => _protoCacheRepository.SetAsync(cacheKey, A<ExampleDetailsDto>._, A<TimeSpan?>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _protoCacheRepository.GetAsync<ExampleProjection>(projectionCacheKey)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _protoCacheRepository.GetAsync<ExampleDetailsDto>(cacheKey)).MustHaveHappenedOnceExactly();

        result.Should()
            .NotBeNull()
            .And.BeOfType<ExampleDetailsDto>()
            .And.BeEquivalentTo(projection, i => i.ExcludingMissingMembers());
    }

    [Test]
    public async Task GetAsync_Without_Projection()
    {
        // Arrange
        var id = _fixture.Create<Guid>();
        var cacheKey = ApplicationConstants.ExampleDetailsCacheKey(id);
        var projectionCacheKey = ApplicationConstants.ExampleProjectionCacheKey(id);

        A.CallTo(() => _protoCacheRepository.GetAsync<ExampleProjection>(projectionCacheKey)).ReturnsLazily(() => default(ExampleProjection));
        A.CallTo(() => _protoCacheRepository.GetAsync<ExampleDetailsDto>(cacheKey)).Returns(default(ExampleDetailsDto));

        // Act
        var result = await _subjectUnderTest.GetAsync(id);

        // Assert
        A.CallTo(() => _protoCacheRepository.SetAsync(cacheKey, A<ExampleDetailsDto>._, A<TimeSpan?>._)).MustNotHaveHappened();
        A.CallTo(() => _protoCacheRepository.GetAsync<ExampleProjection>(projectionCacheKey)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _protoCacheRepository.GetAsync<ExampleDetailsDto>(cacheKey)).MustHaveHappenedOnceExactly();

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

        capturedCommand.Should()
            .NotBeNull()
            .And.BeEquivalentTo(dto);
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

        capturedCommand.Should()
            .NotBeNull()
            .And.BeEquivalentTo(dto);
        capturedCommand.Id.Should().NotBeEmpty().And.Be(id);
    }
}