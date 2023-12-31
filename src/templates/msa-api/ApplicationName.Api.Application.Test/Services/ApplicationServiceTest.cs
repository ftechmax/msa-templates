﻿using ApplicationName.Api.Application.Documents;
using ApplicationName.Api.Application.Repositories;
using ApplicationName.Api.Application.Services;
using ApplicationName.Api.Contracts;
using ApplicationName.Api.Contracts.Commands;
using ApplicationName.Api.Contracts.Documents;
using ApplicationName.Api.Contracts.Dtos;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using AutoMapper;
using FakeItEasy;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using NUnit.Framework;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ApplicationName.Api.Application.Test.Services;

public class ApplicationServiceTest
{
    private IFixture _fixture;

    private IDocumentRepository _documentRepository;

    private IProtoCacheRepository _protoCacheRepository;

    private IMapper _mapper;

    private ISendEndpoint _sendEndpoint;

    private ApplicationService _subjectUnderTest;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization());

        _documentRepository = _fixture.Freeze<IDocumentRepository>();
        _protoCacheRepository = _fixture.Freeze<IProtoCacheRepository>();
        _sendEndpoint = _fixture.Freeze<ISendEndpoint>();

        var mappingConfig = new MapperConfiguration(cfg => { cfg.AddProfile(new MappingProfile()); });
        _mapper = mappingConfig.CreateMapper();
        _fixture.Register(() => _mapper);

        _subjectUnderTest = _fixture.Create<ApplicationService>();
    }

    [Test]
    public async Task GetAsync_From_Cache()
    {
        // Arrange
        var request = _fixture.Create<GetExampleDto>();
        var dto = _fixture.Create<ExampleResultDto>();

        A.CallTo(() => _protoCacheRepository.GetAsync<ExampleResultDto>(A<string>._)).ReturnsLazily(() => dto);

        // Act
        var result = await _subjectUnderTest.GetAsync(request);

        // Assert
        A.CallTo(() => _protoCacheRepository.GetAsync<ExampleResultDto>($"{ApplicationConstants.ExampleCacheKey}_{request.Id:D}")).MustHaveHappenedOnceExactly();
        A.CallTo(() => _documentRepository.GetAsync<ExampleDocument>(A<Guid>._)).MustNotHaveHappened();

        result.Should()
            .NotBeNull()
            .And.BeEquivalentTo(dto);
    }

    [Test]
    public async Task GetAsync_From_Repository()
    {
        // Arrange
        var request = _fixture.Create<GetExampleDto>();
        var document = GenerateDocument();

        A.CallTo(() => _documentRepository.GetAsync<ExampleDocument>(A<Guid>._)).ReturnsLazily(() => document);
        A.CallTo(() => _protoCacheRepository.GetAsync<ExampleResultDto>(A<string>._)).Returns(default(ExampleResultDto));

        // Act
        var result = await _subjectUnderTest.GetAsync(request);

        // Assert
        A.CallTo(() => _protoCacheRepository.SetAsync(A<string>._, A<ExampleResultDto>._, A<DistributedCacheEntryOptions>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _protoCacheRepository.GetAsync<ExampleResultDto>($"{ApplicationConstants.ExampleCacheKey}_{request.Id:D}")).MustHaveHappenedOnceExactly();
        A.CallTo(() => _documentRepository.GetAsync<ExampleDocument>(request.Id)).MustHaveHappenedOnceExactly();

        result.Should()
            .NotBeNull()
            .And.BeOfType<ExampleResultDto>()
            .And.BeEquivalentTo(document, i => i.ExcludingMissingMembers());
    }

    [Test]
    public async Task GetAsync_Without_Document()
    {
        // Arrange
        var request = _fixture.Create<GetExampleDto>();

        A.CallTo(() => _documentRepository.GetAsync<ExampleDocument>(A<Guid>._)).ReturnsLazily(() => default(ExampleDocument));
        A.CallTo(() => _protoCacheRepository.GetAsync<ExampleResultDto>(A<string>._)).Returns(default(ExampleResultDto));

        // Act
        var result = await _subjectUnderTest.GetAsync(request);

        // Assert
        A.CallTo(() => _protoCacheRepository.SetAsync(A<string>._, A<ExampleResultDto>._, A<DistributedCacheEntryOptions>._)).MustNotHaveHappened();
        A.CallTo(() => _protoCacheRepository.GetAsync<ExampleResultDto>(A<string>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _documentRepository.GetAsync<ExampleDocument>(A<Guid>._)).MustHaveHappenedOnceExactly();

        result.Should().BeNull();
    }

    [Test]
    public async Task HandleAsync_CreateExampleDto()
    {
        // Arrange
        var request = _fixture.Create<CreateExampleDto>();

        var capturedCommand = default(IExampleCommand);
        A.CallTo(() => _sendEndpoint.Send(A<IExampleCommand>._, A<CancellationToken>._)).Invokes(
            (IExampleCommand arg0, CancellationToken _) =>
            {
                capturedCommand = arg0;
            });

        // Act
        await _subjectUnderTest.HandleAsync(request);

        // Assert
        A.CallTo(() => _sendEndpoint.Send(A<IExampleCommand>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();

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
            configure.CreateMap<IDocumentBase, ExampleDocument>();
        }).CreateMapper();

        mapper.Map(A.Dummy<IDocumentBase>(), instance);

        var propertyInfo = instance.GetType().GetProperty(nameof(ExampleDocument.Name));
        propertyInfo.SetValue(instance, Convert.ChangeType(_fixture.Create<string>(), propertyInfo.PropertyType), null);

        return instance;
    }
}