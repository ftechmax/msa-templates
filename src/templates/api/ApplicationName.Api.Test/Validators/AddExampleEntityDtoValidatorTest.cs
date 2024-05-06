using ApplicationName.Api.Contracts.Dtos;
using ApplicationName.Api.Validators;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FluentAssertions;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace ApplicationName.Api.Test.Validators;

public class AddExampleEntityDtoValidatorTest
{
    private IFixture _fixture;

    private AddExampleEntityDtoValidator _subjectUnderTest;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization());

        _subjectUnderTest = _fixture.Create<AddExampleEntityDtoValidator>();
    }

    [Test]
    public void Validate_With_Valid_Dto()
    {
        // Arrange
        var dto = _fixture.Create<AddExampleEntityDto>();

        // Act
        var result = _subjectUnderTest.TestValidate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Test]
    public void Validate_With_Invalid_CorrelationId()
    {
        // Arrange
        var dto = new AddExampleEntityDto
        {
            CorrelationId = Guid.Empty,
            Id = _fixture.Create<Guid>(),
            Name = _fixture.Create<string>(),
            SomeValue = _fixture.Create<float>()
        };

        // Act
        var result = _subjectUnderTest.TestValidate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(i => i.CorrelationId);
        result.ShouldNotHaveValidationErrorFor(i => i.Id);
        result.ShouldNotHaveValidationErrorFor(i => i.Name);
        result.ShouldNotHaveValidationErrorFor(i => i.SomeValue);
    }

    [Test]
    public void Validate_With_Invalid_Id()
    {
        // Arrange
        var dto = new AddExampleEntityDto
        {
            CorrelationId = _fixture.Create<Guid>(),
            Id = Guid.Empty,
            Name = _fixture.Create<string>(),
            SomeValue = _fixture.Create<float>()
        };

        // Act
        var result = _subjectUnderTest.TestValidate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldNotHaveValidationErrorFor(i => i.CorrelationId);
        result.ShouldHaveValidationErrorFor(i => i.Id);
        result.ShouldNotHaveValidationErrorFor(i => i.Name);
        result.ShouldNotHaveValidationErrorFor(i => i.SomeValue);
    }

    [Test]
    [TestCase(default(string))]
    [TestCase("")]
    [TestCase(" ")]
    public void Validate_With_Invalid_Name(string testCase)
    {
        // Arrange
        var dto = new AddExampleEntityDto
        {
            CorrelationId = _fixture.Create<Guid>(),
            Id = _fixture.Create<Guid>(),
            Name = testCase,
            SomeValue = _fixture.Create<float>()
        };

        // Act
        var result = _subjectUnderTest.TestValidate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldNotHaveValidationErrorFor(i => i.CorrelationId);
        result.ShouldNotHaveValidationErrorFor(i => i.Id);
        result.ShouldHaveValidationErrorFor(i => i.Name);
        result.ShouldNotHaveValidationErrorFor(i => i.SomeValue);
    }

    [Test]
    [TestCase(-1f)]
    [TestCase(float.NegativeZero)]
    [TestCase(float.NegativeInfinity)]
    public void Validate_With_Invalid_SomeValue(float testCase)
    {
        // Arrange
        var dto = new AddExampleEntityDto
        {
            CorrelationId = _fixture.Create<Guid>(),
            Id = _fixture.Create<Guid>(),
            Name = _fixture.Create<string>(),
            SomeValue = testCase
        };

        // Act
        var result = _subjectUnderTest.TestValidate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldNotHaveValidationErrorFor(i => i.CorrelationId);
        result.ShouldNotHaveValidationErrorFor(i => i.Id);
        result.ShouldNotHaveValidationErrorFor(i => i.Name);
        result.ShouldHaveValidationErrorFor(i => i.SomeValue);
    }
}