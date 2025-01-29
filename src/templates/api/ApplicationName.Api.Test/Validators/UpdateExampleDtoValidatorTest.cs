using ApplicationName.Api.Contracts.Dtos;
using ApplicationName.Api.Contracts.Test;
using ApplicationName.Api.Validators;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FluentValidation.TestHelper;
using NUnit.Framework;
using Shouldly;

namespace ApplicationName.Api.Test.Validators;

public class UpdateExampleDtoValidatorTest
{
    private IFixture _fixture;

    private UpdateExampleDtoValidator _subjectUnderTest;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization());

        _subjectUnderTest = _fixture.Create<UpdateExampleDtoValidator>();
    }

    [Test]
    public void Validate_With_Valid_Dto()
    {
        // Arrange
        var dto = _fixture.Create<UpdateExampleDto>();

        // Act
        var result = _subjectUnderTest.TestValidate(dto);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public void Validate_With_Invalid_CorrelationId()
    {
        // Arrange
        var dto = _fixture.Create<UpdateExampleDto>() with { CorrelationId = Guid.Empty };

        // Act
        var result = _subjectUnderTest.TestValidate(dto);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.ShouldHaveValidationErrorFor(i => i.CorrelationId);
        result.ShouldNotHaveValidationErrorFor(i => i.Description);
        result.ShouldNotHaveValidationErrorFor(i => i.ExampleValueObject);
    }

    [Test]
    [TestCaseSource(typeof(TestCases), nameof(TestCases.StringCases))]
    public void Validate_With_Invalid_Description(string testCase)
    {
        // Arrange
        var dto = _fixture.Create<UpdateExampleDto>() with { Description = testCase };

        // Act
        var result = _subjectUnderTest.TestValidate(dto);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.ShouldNotHaveValidationErrorFor(i => i.CorrelationId);
        result.ShouldHaveValidationErrorFor(i => i.Description);
        result.ShouldNotHaveValidationErrorFor(i => i.ExampleValueObject);
    }

    [Test]
    public void Validate_With_Invalid_ExampleValueObject()
    {
        // Arrange
        var dto = _fixture.Create<UpdateExampleDto>() with { ExampleValueObject = default };

        // Act
        var result = _subjectUnderTest.TestValidate(dto);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.ShouldNotHaveValidationErrorFor(i => i.CorrelationId);
        result.ShouldNotHaveValidationErrorFor(i => i.Description);
        result.ShouldHaveValidationErrorFor(i => i.ExampleValueObject);
    }
}