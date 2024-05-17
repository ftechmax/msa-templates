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

    private ISendEndpointProvider _sendEndpointProvider;

    private ISendEndpoint _sendEndpoint;

    private ExampleService _subjectUnderTest;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization());

        _protoCacheRepository = _fixture.Freeze<IProtoCacheRepository>();

        _sendEndpointProvider = _fixture.Freeze<ISendEndpointProvider>();
        _sendEndpoint = _fixture.Freeze<ISendEndpoint>();
        A.CallTo(() => _sendEndpointProvider.GetSendEndpoint(ApplicationConstants.MessageEndpoint)).Returns(_sendEndpoint);

        var mappingConfig = new MapperConfiguration(cfg => { cfg.AddProfile(new MappingProfile()); });
        _mapper = mappingConfig.CreateMapper();
        _fixture.Register(() => _mapper);

        _subjectUnderTest = _fixture.Create<ExampleService>();
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
        A.CallTo(() => _protoCacheRepository.GetAsync<ExampleProjection>($"{nameof(ExampleProjection)}:{id:N}")).MustNotHaveHappened();

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

        A.CallTo(() => _protoCacheRepository.GetAsync<ExampleProjection>($"{nameof(ExampleProjection)}:{id:N}")).ReturnsLazily(() => projection);
        A.CallTo(() => _protoCacheRepository.GetAsync<ExampleDetailsDto>(cacheKey)).Returns(default(ExampleDetailsDto));

        // Act
        var result = await _subjectUnderTest.GetAsync(id);

        // Assert
        A.CallTo(() => _protoCacheRepository.SetAsync(A<string>._, A<ExampleDetailsDto>._, A<TimeSpan>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _protoCacheRepository.GetAsync<ExampleDetailsDto>(cacheKey)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _protoCacheRepository.GetAsync<ExampleProjection>($"{nameof(ExampleProjection)}:{id:N}")).MustHaveHappenedOnceExactly();

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

        A.CallTo(() => _protoCacheRepository.GetAsync<ExampleProjection>($"{nameof(ExampleProjection)}:{id:N}")).ReturnsLazily(() => default(ExampleProjection));
        A.CallTo(() => _protoCacheRepository.GetAsync<ExampleDetailsDto>(A<string>._)).Returns(default(ExampleDetailsDto));

        // Act
        var result = await _subjectUnderTest.GetAsync(id);

        // Assert
        A.CallTo(() => _protoCacheRepository.SetAsync(A<string>._, A<ExampleDetailsDto>._, A<TimeSpan>._)).MustNotHaveHappened();
        A.CallTo(() => _protoCacheRepository.GetAsync<ExampleDetailsDto>(A<string>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _protoCacheRepository.GetAsync<ExampleProjection>($"{nameof(ExampleProjection)}:{id:N}")).MustHaveHappenedOnceExactly();

        result.Should().BeNull();
    }

    [Test]
    public async Task HandleAsync_CreateExampleDto()
    {
        // Arrange
        var request = _fixture.Create<CreateExampleDto>();

        var capturedCommand = default(ICreateExampleCommand);
        A.CallTo(() => _sendEndpoint.Send(A<ICreateExampleCommand>._, A<CancellationToken>._)).Invokes(
            (ICreateExampleCommand arg0, CancellationToken _) =>
            {
                capturedCommand = arg0;
            });

        // Act
        await _subjectUnderTest.HandleAsync(request);

        // Assert
        A.CallTo(() => _sendEndpoint.Send(A<ICreateExampleCommand>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();

        capturedCommand.Should().NotBeNull();
        capturedCommand.Name.Should().NotBeNullOrWhiteSpace().And.Be(request.Name);
    }
}