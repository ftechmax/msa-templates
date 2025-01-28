using ApplicationName.Api.Application.Services;
using ApplicationName.Api.Contracts.Dtos;
using ApplicationName.Api.Controllers;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using Shouldly;
using NUnit.Framework;

namespace ApplicationName.Api.Test.Controllers;

public class ExampleControllerTest
{
    private IFixture _fixture;

    private IExampleService _applicationService;

    private ExampleController _subjectUnderTest;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization());
        _applicationService = _fixture.Freeze<IExampleService>();
        _subjectUnderTest = _fixture.Build<ExampleController>().OmitAutoProperties().Create();
    }

    [Test]
    public async Task GetCollection()
    {
        // Arrange
        var dtos = _fixture.CreateMany<ExampleCollectionDto>();

        A.CallTo(() => _applicationService.GetCollectionAsync()).ReturnsLazily(() => dtos);

        // Act
        var result = await _subjectUnderTest.GetCollection();

        // Assert
        A.CallTo(() => _applicationService.GetCollectionAsync()).MustHaveHappenedOnceExactly();

        result.Should()
            .NotBeNullOrEmpty()
            .And.HaveSameCount(dtos)
            .And.Contain(dtos);
    }

    [Test]
    public async Task Get()
    {
        // Arrange
        var id = _fixture.Create<Guid>();
        var returnDto = _fixture.Create<ExampleDetailsDto>();

        A.CallTo(() => _applicationService.GetAsync(id)).ReturnsLazily(() => returnDto);

        // Act
        var result = await _subjectUnderTest.Get(id);

        // Assert
        A.CallTo(() => _applicationService.GetAsync(id)).MustHaveHappenedOnceExactly();

        result.Should()
            .NotBeNull()
            .And.Be(returnDto);
    }

    [Test]
    public async Task Post()
    {
        // Arrange
        var dto = _fixture.Create<CreateExampleDto>();

        // Act
        await _subjectUnderTest.Post(dto);

        // Assert
        A.CallTo(() => _applicationService.HandleAsync(dto)).MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task Put()
    {
        // Arrange
        var id = _fixture.Create<Guid>();
        var dto = _fixture.Create<UpdateExampleDto>();

        // Act
        await _subjectUnderTest.Put(id, dto);

        // Assert
        A.CallTo(() => _applicationService.HandleAsync(id, dto)).MustHaveHappenedOnceExactly();
    }
}