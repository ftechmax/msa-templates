using System;
using System.Threading.Tasks;
using ApplicationName.Api.Application.Services;
using ApplicationName.Api.Contracts.Dtos;
using ApplicationName.Api.Controllers;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using FluentAssertions;
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
    public async Task Get_With_Valid_Dto()
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
    public async Task Post_With_Valid_Dto()
    {
        // Arrange
        var dto = _fixture.Create<CreateExampleDto>();

        // Act
        await _subjectUnderTest.Post(dto);

        // Assert
        A.CallTo(() => _applicationService.HandleAsync(dto)).MustHaveHappenedOnceExactly();
    }
}