using ApplicationName.Api.Contracts.Dtos;
using ApplicationName.Api.Validators;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FluentAssertions;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace ApplicationName.Api.Test.Validators;

public class UpdateExampleEntityDtoValidatorTest
{
    private IFixture _fixture;

    private UpdateExampleEntityDtoValidator _subjectUnderTest;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization());

        _subjectUnderTest = _fixture.Create<UpdateExampleEntityDtoValidator>();
    }

    [Test]
    public void Validate_With_Valid_Dto()
    {
        // Arrange
        var dto = _fixture.Create<UpdateExampleEntityDto>();

        // Act
        var result = _subjectUnderTest.TestValidate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Test]
    public void Validate_With_Invalid_CorrelationId()
    {
        // Arrange
        var dto = new UpdateExampleEntityDto
        {
            CorrelationId = Guid.Empty,
            SomeValue = _fixture.Create<float>()
        };

        // Act
        var result = _subjectUnderTest.TestValidate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(i => i.CorrelationId);
        result.ShouldNotHaveValidationErrorFor(i => i.SomeValue);
    }

    [Test]
    [TestCase(-1f)]
    [TestCase(float.NegativeZero)]
    [TestCase(float.NegativeInfinity)]
    public void Validate_With_Invalid_SomeValue(float testCase)
    {
        // Arrange
        var dto = new UpdateExampleEntityDto
        {
            CorrelationId = _fixture.Create<Guid>(),
            SomeValue = testCase
        };

        // Act
        var result = _subjectUnderTest.TestValidate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldNotHaveValidationErrorFor(i => i.CorrelationId);
        result.ShouldHaveValidationErrorFor(i => i.SomeValue);
    }
}