using ApplicationName.Api.Contracts.Dtos;
using ApplicationName.Api.Contracts.Test;
using ApplicationName.Api.Validators;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FluentAssertions;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace ApplicationName.Api.Test.Validators;

public class ExampleValueObjectDtoValidatorTest
{
    private IFixture _fixture;

    private ExampleValueObjectDtoValidator _subjectUnderTest;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization());

        _subjectUnderTest = _fixture.Create<ExampleValueObjectDtoValidator>();
    }

    [Test]
    public void Validate_With_Valid_Dto()
    {
        // Arrange
        var dto = _fixture.Create<ExampleValueObjectDto>();

        // Act
        var result = _subjectUnderTest.TestValidate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Test]
    [TestCaseSource(typeof(TestCases), nameof(TestCases.StringCases))]
    public void Validate_With_Invalid_Code(string testCase)
    {
        // Arrange
        var dto = _fixture.Create<ExampleValueObjectDto>() with { Code = testCase };

        // Act
        var result = _subjectUnderTest.TestValidate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(i => i.Code);
        result.ShouldNotHaveValidationErrorFor(i => i.Value);
    }

    [Test]
    [TestCaseSource(typeof(TestCases), nameof(TestCases.NegativeDoubleCases))]
    public void Validate_With_Invalid_Value(double testCase)
    {
        // Arrange
        var dto = _fixture.Create<ExampleValueObjectDto>() with { Value = testCase };

        // Act
        var result = _subjectUnderTest.TestValidate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldNotHaveValidationErrorFor(i => i.Code);
        result.ShouldHaveValidationErrorFor(i => i.Value);
    }
}