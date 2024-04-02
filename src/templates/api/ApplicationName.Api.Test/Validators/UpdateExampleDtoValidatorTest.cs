using ApplicationName.Api.Contracts.Dtos;
using ApplicationName.Api.Validators;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FluentAssertions;
using FluentValidation.TestHelper;
using NUnit.Framework;

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
        result.IsValid.Should().BeTrue();
    }

    [Test]
    public void Validate_With_Invalid_Id()
    {
        // Arrange
        var dto = _fixture.Create<UpdateExampleDto>();
        dto.Id = Guid.Empty;

        // Act
        var result = _subjectUnderTest.TestValidate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(i => i.Id);
    }
}