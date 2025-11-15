using System.Linq.Expressions;
using ApplicationName.Api.Application.Repositories;
using ApplicationName.Api.Application.Services;
using ApplicationName.Api.Contracts;
using ApplicationName.Api.Contracts.Dtos;
using ApplicationName.Shared.Aggregates;
using ApplicationName.Shared.Commands;
using ApplicationName.Shared.Projections;
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

        var mapperConfig = new TypeAdapterConfig();
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
        A.CallTo(() => _protoCacheRepository.GetAllAsync<ExampleProjection>(ApplicationConstants.ExampleProjectionCacheKey)).MustNotHaveHappened();

        result.ShouldSatisfyAllConditions(
            i => i.ShouldNotBeNull(),
            i => i.ShouldNotBeEmpty(),
            i => i.Count().ShouldBe(dtos.Count()),
            i => i.ShouldBeSameAs(dtos));
    }

    [Test]
    public async Task GetCollectionAsync_From_Projection()
    {
        // Arrange
        var projection1 = _fixture.Create<ExampleProjection>();
        var projection2 = _fixture.Create<ExampleProjection>();
        var projection3 = _fixture.Create<ExampleProjection>();
        var projections = new[] { projection1, projection2, projection3 };

        A.CallTo(() => _protoCacheRepository.GetAsync<IEnumerable<ExampleCollectionDto>>(ApplicationConstants.ExampleCollectionCacheKey))
            .ReturnsLazily(() => default(IEnumerable<ExampleCollectionDto>));
        A.CallTo(() => _protoCacheRepository.GetAllAsync<ExampleProjection>(ApplicationConstants.ExampleProjectionCacheKey))
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
        A.CallTo(() => _protoCacheRepository.GetAllAsync<ExampleProjection>(ApplicationConstants.ExampleProjectionCacheKey)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _protoCacheRepository.SetAsync(ApplicationConstants.ExampleCollectionCacheKey, A<IEnumerable<ExampleCollectionDto>>._, A<TimeSpan?>._)).MustHaveHappenedOnceExactly();

        capturedDtos.ShouldSatisfyAllConditions(
            i => i.ShouldNotBeNull(),
            i => i.ShouldNotBeEmpty(),
            i => i.Count().ShouldBe(projections.Length),
            i => i.ShouldBe(expectedDtos));

        result.ShouldSatisfyAllConditions(
            i => i.ShouldNotBeNull(),
            i => i.ShouldNotBeEmpty(),
            i => i.Count().ShouldBe(projections.Length),
            i => i.ShouldBe(expectedDtos));
    }

    [Test]
    public async Task GetCollectionAsync_From_Repository_Without_Projection()
    {
        // Arrange
        A.CallTo(() => _protoCacheRepository.GetAsync<IEnumerable<ExampleCollectionDto>>(ApplicationConstants.ExampleCollectionCacheKey)).ReturnsLazily(() => default(IEnumerable<ExampleCollectionDto>));
        A.CallTo(() => _protoCacheRepository.GetAllAsync<ExampleProjection>(ApplicationConstants.ExampleProjectionCacheKey)).Returns([]);

        // Act
        var result = await _subjectUnderTest.GetCollectionAsync();

        // Assert
        A.CallTo(() => _protoCacheRepository.SetAsync(A<string>._, A<IEnumerable<ExampleCollectionDto>>._, A<TimeSpan?>._)).MustNotHaveHappened();
        A.CallTo(() => _protoCacheRepository.GetAsync<IEnumerable<ExampleCollectionDto>>(A<string>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _protoCacheRepository.GetAllAsync<ExampleProjection>(ApplicationConstants.ExampleProjectionCacheKey)).MustHaveHappenedOnceExactly();

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
        A.CallTo(() => _protoCacheRepository.GetAsync<ExampleProjection>(ApplicationConstants.ExampleProjectionByIdCacheKey(id))).MustNotHaveHappened();

        result.ShouldSatisfyAllConditions(
            i => i.ShouldNotBeNull(),
            i => i.ShouldBeSameAs(dto));
    }

    [Test]
    public async Task GetAsync_From_Projection()
    {
        // Arrange
        var id = _fixture.Create<Guid>();
        var projection = _fixture.Create<ExampleProjection>() with { Id = id };
        var cacheKey = ApplicationConstants.ExampleDetailsCacheKey(id);
        var projectionKey = ApplicationConstants.ExampleProjectionByIdCacheKey(id);

        A.CallTo(() => _protoCacheRepository.GetAsync<ExampleProjection>(projectionKey)).ReturnsLazily(() => projection);
        A.CallTo(() => _protoCacheRepository.GetAsync<ExampleDetailsDto>(cacheKey)).Returns(default(ExampleDetailsDto));

        // Act
        var result = await _subjectUnderTest.GetAsync(id);

        // Assert
        A.CallTo(() => _protoCacheRepository.SetAsync(A<string>._, A<ExampleDetailsDto>._, A<TimeSpan?>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _protoCacheRepository.GetAsync<ExampleDetailsDto>(cacheKey)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _protoCacheRepository.GetAsync<ExampleProjection>(projectionKey)).MustHaveHappenedOnceExactly();

        result.ShouldSatisfyAllConditions(
            i => i.ShouldNotBeNull(),
            i => i.ShouldBeOfType<ExampleDetailsDto>(),
            i => i.Id.ShouldBe(projection.Id),
            i => i.Name.ShouldBe(projection.Name),
            i => i.Description.ShouldBe(projection.Description),
            i => i.ExampleValueObject.ShouldSatisfyAllConditions(
                j => j.ShouldNotBeNull(),
                j => j.Code.ShouldBe(projection.ExampleValueObject.Code),
                j => j.Value.ShouldBe(projection.ExampleValueObject.Value)),
            i => i.RemoteCode.ShouldBe(projection.RemoteCode));
    }

    [Test]
    public async Task GetAsync_Without_Projection()
    {
        // Arrange
        var id = _fixture.Create<Guid>();

        A.CallTo(() => _protoCacheRepository.GetAsync<ExampleProjection>(ApplicationConstants.ExampleProjectionByIdCacheKey(id))).ReturnsLazily(() => default(ExampleProjection));
        A.CallTo(() => _protoCacheRepository.GetAsync<ExampleDetailsDto>(A<string>._)).Returns(default(ExampleDetailsDto));

        // Act
        var result = await _subjectUnderTest.GetAsync(id);

        // Assert
        A.CallTo(() => _protoCacheRepository.SetAsync(A<string>._, A<ExampleDetailsDto>._, A<TimeSpan?>._)).MustNotHaveHappened();
        A.CallTo(() => _protoCacheRepository.GetAsync<ExampleDetailsDto>(A<string>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _protoCacheRepository.GetAsync<ExampleProjection>(ApplicationConstants.ExampleProjectionByIdCacheKey(id))).MustHaveHappenedOnceExactly();

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
}