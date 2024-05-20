using ApplicationName.Api.Contracts.Dtos;
using ApplicationName.Api.Validators;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FluentAssertions;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace ApplicationName.Api.Test.Validators;

public class CreateExampleDtoValidatorTest
{
    private IFixture _fixture;

    private CreateExampleDtoValidator _subjectUnderTest;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization());

        _subjectUnderTest = _fixture.Create<CreateExampleDtoValidator>();
    }

    [Test]
    public void Validate_With_Valid_Dto()
    {
        // Arrange
        var dto = _fixture.Create<CreateExampleDto>();

        // Act
        var result = _subjectUnderTest.TestValidate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Test]
    public void Validate_With_Invalid_CorrelationId()
    {
        // Arrange
        var dto = new CreateExampleDto
        {
            CorrelationId = Guid.Empty,
            Name = _fixture.Create<string>(),
            Description = _fixture.Create<string>(),
            ExampleValueObject = _fixture.Create<ExampleValueObjectDto>()
        };

        // Act
        var result = _subjectUnderTest.TestValidate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(i => i.CorrelationId);
        result.ShouldNotHaveValidationErrorFor(i => i.Name);
        result.ShouldNotHaveValidationErrorFor(i => i.Description);
        result.ShouldNotHaveValidationErrorFor(i => i.ExampleValueObject);
    }

    [Test]
    [TestCase(default(string))]
    [TestCase("")]
    [TestCase(" ")]
    public void Validate_With_Invalid_Name(string testCase)
    {
        // Arrange
        var dto = new CreateExampleDto
        {
            CorrelationId = _fixture.Create<Guid>(),
            Name = testCase,
            Description = _fixture.Create<string>(),
            ExampleValueObject = _fixture.Create<ExampleValueObjectDto>()
        };

        // Act
        var result = _subjectUnderTest.TestValidate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldNotHaveValidationErrorFor(i => i.CorrelationId);
        result.ShouldHaveValidationErrorFor(i => i.Name);
        result.ShouldNotHaveValidationErrorFor(i => i.Description);
        result.ShouldNotHaveValidationErrorFor(i => i.ExampleValueObject);
    }

    [Test]
    [TestCase(default(string))]
    [TestCase("")]
    [TestCase(" ")]
    public void Validate_With_Invalid_Description(string testCase)
    {
        // Arrange
        var dto = new CreateExampleDto
        {
            CorrelationId = _fixture.Create<Guid>(),
            Name = _fixture.Create<string>(),
            Description = testCase,
            ExampleValueObject = _fixture.Create<ExampleValueObjectDto>()
        };

        // Act
        var result = _subjectUnderTest.TestValidate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldNotHaveValidationErrorFor(i => i.CorrelationId);
        result.ShouldNotHaveValidationErrorFor(i => i.Name);
        result.ShouldHaveValidationErrorFor(i => i.Description);
        result.ShouldNotHaveValidationErrorFor(i => i.ExampleValueObject);
    }

    [Test]
    public void Validate_With_Invalid_ExampleValueObject()
    {
        // Arrange
        var dto = new CreateExampleDto
        {
            CorrelationId = _fixture.Create<Guid>(),
            Name = _fixture.Create<string>(),
            Description = _fixture.Create<string>(),
            ExampleValueObject = default
        };

        // Act
        var result = _subjectUnderTest.TestValidate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldNotHaveValidationErrorFor(i => i.CorrelationId);
        result.ShouldNotHaveValidationErrorFor(i => i.Name);
        result.ShouldNotHaveValidationErrorFor(i => i.Description);
        result.ShouldHaveValidationErrorFor(i => i.ExampleValueObject);
    }
}