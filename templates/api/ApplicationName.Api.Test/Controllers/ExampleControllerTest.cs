using ApplicationName.Api.Application.Services;
using ApplicationName.Api.Contracts.Dtos;
using ApplicationName.Api.Controllers;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NUnit.Framework;
using Shouldly;

namespace ApplicationName.Api.Test.Controllers;

public class ExampleControllerTest
{
    private IFixture _fixture;

    private IExampleService _applicationService;

    private IValidator<CreateExampleDto> _createExampleDtoValidator;

    private IValidator<UpdateExampleDto> _updateExampleDtoValidator;

    private ExampleController _subjectUnderTest;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization());
        _applicationService = _fixture.Freeze<IExampleService>();
        _createExampleDtoValidator = _fixture.Freeze<IValidator<CreateExampleDto>>();
        _updateExampleDtoValidator = _fixture.Freeze<IValidator<UpdateExampleDto>>();
        _subjectUnderTest = _fixture.Build<ExampleController>().OmitAutoProperties().Create();

        // ValidationProblem() resolves this from HttpContext.RequestServices, which a unit test has no request for.
        _subjectUnderTest.ProblemDetailsFactory = new TestProblemDetailsFactory();
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

        result.ShouldSatisfyAllConditions(
            i => i.ShouldNotBeNull(),
            i => i.ShouldNotBeEmpty(),
            i => i.Count().ShouldBe(dtos.Count()),
            i => i.ShouldBeSameAs(dtos));
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

        result.ShouldSatisfyAllConditions(
            i => i.ShouldNotBeNull(),
            i => i.ShouldBeSameAs(returnDto));
    }

    [Test]
    public async Task Post()
    {
        // Arrange
        var dto = _fixture.Create<CreateExampleDto>();

        A.CallTo(() => _createExampleDtoValidator.ValidateAsync(dto, A<CancellationToken>._))
            .ReturnsLazily(() => new ValidationResult());

        // Act
        var result = await _subjectUnderTest.Post(dto);

        // Assert
        A.CallTo(() => _applicationService.HandleAsync(dto)).MustHaveHappenedOnceExactly();

        result.ShouldBeOfType<AcceptedResult>();
    }

    [Test]
    public async Task Post_With_Invalid_Dto()
    {
        // Arrange
        var dto = _fixture.Create<CreateExampleDto>();

        A.CallTo(() => _createExampleDtoValidator.ValidateAsync(dto, A<CancellationToken>._))
            .ReturnsLazily(() => new ValidationResult([new ValidationFailure(nameof(CreateExampleDto.Name), "'Name' must not be empty.")]));

        // Act
        var result = await _subjectUnderTest.Post(dto);

        // Assert
        A.CallTo(() => _applicationService.HandleAsync(A<CreateExampleDto>._)).MustNotHaveHappened();

        var problemDetails = result.ShouldBeOfType<BadRequestObjectResult>().Value.ShouldBeOfType<ValidationProblemDetails>();
        problemDetails.Errors.ShouldContainKey(nameof(CreateExampleDto.Name));
    }

    [Test]
    public async Task Put()
    {
        // Arrange
        var id = _fixture.Create<Guid>();
        var dto = _fixture.Create<UpdateExampleDto>();

        A.CallTo(() => _updateExampleDtoValidator.ValidateAsync(dto, A<CancellationToken>._))
            .ReturnsLazily(() => new ValidationResult());

        // Act
        var result = await _subjectUnderTest.Put(id, dto);

        // Assert
        A.CallTo(() => _applicationService.HandleAsync(id, dto)).MustHaveHappenedOnceExactly();

        result.ShouldBeOfType<AcceptedResult>();
    }

    [Test]
    public async Task Put_With_Invalid_Dto()
    {
        // Arrange
        var id = _fixture.Create<Guid>();
        var dto = _fixture.Create<UpdateExampleDto>();

        A.CallTo(() => _updateExampleDtoValidator.ValidateAsync(dto, A<CancellationToken>._))
            .ReturnsLazily(() => new ValidationResult([new ValidationFailure(nameof(UpdateExampleDto.Description), "'Description' must not be empty.")]));

        // Act
        var result = await _subjectUnderTest.Put(id, dto);

        // Assert
        A.CallTo(() => _applicationService.HandleAsync(A<Guid>._, A<UpdateExampleDto>._)).MustNotHaveHappened();

        var problemDetails = result.ShouldBeOfType<BadRequestObjectResult>().Value.ShouldBeOfType<ValidationProblemDetails>();
        problemDetails.Errors.ShouldContainKey(nameof(UpdateExampleDto.Description));
    }

    private sealed class TestProblemDetailsFactory : ProblemDetailsFactory
    {
        public override ProblemDetails CreateProblemDetails(HttpContext httpContext, int? statusCode = null,
            string title = null, string type = null, string detail = null, string instance = null)
        {
            return new ProblemDetails { Status = statusCode ?? StatusCodes.Status500InternalServerError };
        }

        public override ValidationProblemDetails CreateValidationProblemDetails(HttpContext httpContext,
            ModelStateDictionary modelStateDictionary, int? statusCode = null, string title = null,
            string type = null, string detail = null, string instance = null)
        {
            return new ValidationProblemDetails(modelStateDictionary)
            {
                Status = statusCode ?? StatusCodes.Status400BadRequest
            };
        }
    }
}
