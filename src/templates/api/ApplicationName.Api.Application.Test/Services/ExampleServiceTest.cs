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

    private ISendEndpointProvider _sendEndpointProvider;

    private ISendEndpoint _sendEndpoint;

    private ExampleService _subjectUnderTest;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization());

        _documentRepository = _fixture.Freeze<IDocumentRepository>();
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
        A.CallTo(() => _documentRepository.GetAsync(A<Expression<Func<ExampleDocument, bool>>>._)).MustNotHaveHappened();

        result.Should()
            .NotBeNull()
            .And.BeEquivalentTo(dto);
    }

    [Test]
    public async Task GetAsync_From_Repository()
    {
        // Arrange
        var id = _fixture.Create<Guid>();
        var document = GenerateDocument();
        var cacheKey = ApplicationConstants.ExampleDetailsCacheKey(id);

        A.CallTo(() => _documentRepository.GetAsync(A<Expression<Func<ExampleDocument, bool>>>._)).ReturnsLazily(() => document);
        A.CallTo(() => _protoCacheRepository.GetAsync<ExampleDetailsDto>(cacheKey)).Returns(default(ExampleDetailsDto));

        // Act
        var result = await _subjectUnderTest.GetAsync(id);

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

    private ExampleDocument GenerateDocument()
    {
        var ci = typeof(ExampleDocument).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, Type.EmptyTypes);
        var instance = (ExampleDocument)ci.Invoke(null);

        var mapper = new MapperConfiguration(configure =>
        {
            configure.ShouldMapProperty = i => i.PropertyType.IsPublic || i.PropertyType.IsNotPublic;
            configure.CreateMap<IAggregate, ExampleDocument>();
        }).CreateMapper();

        mapper.Map(A.Dummy<IAggregate>(), instance);

        var propertyInfo = instance.GetType().GetProperty(nameof(ExampleDocument.Name));
        propertyInfo.SetValue(instance, Convert.ChangeType(_fixture.Create<string>(), propertyInfo.PropertyType), null);

        return instance;
    }
}