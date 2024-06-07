using ApplicationName.Shared.Commands;
using ApplicationName.Worker.Application.Documents;
using ApplicationName.Worker.Application.DomainEvents;
using ApplicationName.Worker.Contracts.Test;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FluentAssertions;
using NUnit.Framework;

namespace ApplicationName.Worker.Application.Test.Documents;

internal class ExampleDocumentTest
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
        var command = _fixture.Create<CreateExampleCommand>();

        // Act
        var result = new ExampleDocument(command);

        // Assert
        result.Id.Should().NotBeEmpty();
        result.Created.Should().NotBe(DateTime.MinValue).And.BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        result.Updated.Should().NotBe(DateTime.MinValue).And.Be(result.Created);
        result.Name.Should().NotBeNullOrWhiteSpace().And.Be(command.Name);
        result.ExampleValueObject.Should().NotBeNull().And.BeEquivalentTo(command.ExampleValueObject);
        result.RemoteCode.Should().BeNull();
    }

    [Test]
    [TestCaseSource(typeof(TestCases), nameof(TestCases.StringCases))]
    public void Constructor_With_Invalid_Name(string testCase)
    {
        // Arrange
        var command = _fixture.Create<CreateExampleCommand>() with { Name = testCase };

        // Act
        var act = () => new ExampleDocument(command);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage($"{nameof(command.Name)}*");
    }

    [Test]
    [TestCaseSource(typeof(TestCases), nameof(TestCases.StringCases))]
    public void Constructor_With_Invalid_Description(string testCase)
    {
        // Arrange
        var command = _fixture.Create<CreateExampleCommand>() with { Description = testCase };

        // Act
        var act = () => new ExampleDocument(command);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage($"{nameof(command.Description)}*");
    }

    [Test]
    public void Constructor_With_Invalid_ExampleValueObject()
    {
        // Arrange
        var command = _fixture.Create<CreateExampleCommand>() with { ExampleValueObject = default };

        // Act
        var act = () => new ExampleDocument(command);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage($"{nameof(command.ExampleValueObject)}*");
    }

    [Test]
    public void Handle_UpdateExampleCommand()
    {
        // Arrange
        var initialCommand = _fixture.Create<CreateExampleCommand>();
        var document = new ExampleDocument(initialCommand);

        var command = _fixture.Create<UpdateExampleCommand>() with { Id = document.Id };

        // Act
        var result = document.Handle(command);

        // Assert
        result.Should().NotBeNull().And.BeOfType<ExampleUpdated>();
        result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        result.Id.Should().NotBeEmpty().And.Be(document.Id);
        result.Description.Should().NotBeNullOrWhiteSpace().And.Be(command.Description);
        result.ExampleValueObject.Should().NotBeNull().And.BeEquivalentTo(command.ExampleValueObject);

        document.Updated.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Test]
    public void Handle_UpdateExampleCommand_With_Invalid_Command()
    {
        // Arrange
        var initialCommand = _fixture.Create<CreateExampleCommand>();
        var document = new ExampleDocument(initialCommand);

        var command = default(UpdateExampleCommand);

        // Act
        var act = () => document.Handle(command);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage($"{nameof(command)}*");
    }
}