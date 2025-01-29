using ApplicationName.Shared.Events;
using ApplicationName.Worker.Application.Documents;
using ApplicationName.Worker.Contracts.Test;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using NUnit.Framework;
using Shouldly;

namespace ApplicationName.Worker.Application.Test.Documents;

internal class ExampleValueObjectTest
{
    private IFixture _fixture;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization());
    }

    [Test]
    public void Constructor_With_Valid_Command()
    {
        // Arrange
        var eventData = _fixture.Create<ExampleValueObjectEventData>();

        // Act
        var result = new ExampleValueObject(eventData);

        // Assert
        result.ShouldSatisfyAllConditions(
            i => i.Code.ShouldBe(eventData.Code),
            i => i.Value.ShouldBe(eventData.Value));
    }

    [Test]
    public void Constructor_With_Invalid_Command()
    {
        // Arrange
        var eventData = default(ExampleValueObjectEventData);

        // Act
        var act = () => new ExampleValueObject(eventData);

        // Assert
        act.ShouldThrow<ArgumentException>().Message.ShouldStartWith($"{nameof(eventData)}");
    }

    [Test]
    [TestCaseSource(typeof(TestCases), nameof(TestCases.StringCases))]
    public void Constructor_With_Invalid_Code(string testCase)
    {
        // Arrange
        var command = _fixture.Create<ExampleValueObjectEventData>() with { Code = testCase };

        // Act
        var act = () => new ExampleValueObject(command);

        // Assert
        act.ShouldThrow<ArgumentException>().Message.ShouldStartWith($"{nameof(command.Code)}");
    }

    [Test]
    [TestCaseSource(typeof(TestCases), nameof(TestCases.NegativeDoubleCases))]
    public void Constructor_With_Invalid_Value(double testCase)
    {
        // Arrange
        var command = _fixture.Create<ExampleValueObjectEventData>() with { Value = testCase };

        // Act
        var act = () => new ExampleValueObject(command);

        // Assert
        act.ShouldThrow<ArgumentException>().Message.ShouldStartWith($"{nameof(command.Value)}");
    }
}