using ApplicationName.Shared.Commands;
using ApplicationName.Worker.Application.Documents;
using ApplicationName.Worker.Application.DomainEvents;
using ApplicationName.Worker.Contracts.Commands;
using ApplicationName.Worker.Contracts.Test;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using Shouldly;
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
    public void Constructor_With_Invalid_Command()
    {
        // Arrange
        var command = default(CreateExampleCommand);

        // Act
        var act = () => new ExampleDocument(command);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage($"{nameof(command)}*");
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

    [Test]
    [TestCaseSource(typeof(TestCases), nameof(TestCases.StringCases))]
    public void Handle_UpdateExampleCommand_With_Invalid_Description(string testCase)
    {
        // Arrange
        var initialCommand = _fixture.Create<CreateExampleCommand>();
        var document = new ExampleDocument(initialCommand);

        var command = _fixture.Create<UpdateExampleCommand>() with { Description = testCase };

        // Act
        var act = () => document.Handle(command);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage($"{nameof(command.Description)}*");
    }

    [Test]
    public void Handle_UpdateExampleCommand_With_Invalid_ExampleValueObject()
    {
        // Arrange
        var initialCommand = _fixture.Create<CreateExampleCommand>();
        var document = new ExampleDocument(initialCommand);

        var command = _fixture.Create<UpdateExampleCommand>() with { ExampleValueObject = default };

        // Act
        var act = () => document.Handle(command);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage($"{nameof(command.ExampleValueObject)}*");
    }

    [Test]
    public void Handle_SetExampleRemoteCodeCommand()
    {
        // Arrange
        var initialCommand = _fixture.Create<CreateExampleCommand>();
        var document = new ExampleDocument(initialCommand);

        var command = _fixture.Create<SetExampleRemoteCodeCommand>() with { Id = document.Id };

        // Act
        var result = document.Handle(command);

        // Assert
        result.Should().NotBeNull().And.BeOfType<ExampleRemoteCodeSet>();
        result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        result.Id.Should().NotBeEmpty().And.Be(document.Id);
        result.RemoteCode.Should().Be(command.RemoteCode);

        document.Updated.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Test]
    public void Handle_SetExampleRemoteCodeCommand_With_Invalid_Command()
    {
        // Arrange
        var initialCommand = _fixture.Create<CreateExampleCommand>();
        var document = new ExampleDocument(initialCommand);

        var command = default(SetExampleRemoteCodeCommand);

        // Act
        var act = () => document.Handle(command);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage($"{nameof(command)}*");
    }

    [Test]
    [TestCaseSource(typeof(TestCases), nameof(TestCases.NegativeIntCases))]
    public void Handle_SetExampleRemoteCodeCommand_With_Invalid_Code(int testCase)
    {
        // Arrange
        var initialCommand = _fixture.Create<CreateExampleCommand>();
        var document = new ExampleDocument(initialCommand);

        var command = _fixture.Create<SetExampleRemoteCodeCommand>() with { RemoteCode = testCase };

        // Act
        var act = () => document.Handle(command);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage($"{nameof(command.RemoteCode)}*");
    }

    [Test]
    public void Create()
    {
        // Arrange
        var command = _fixture.Create<CreateExampleCommand>();

        // Act
        var (document, domainEvent) = ExampleDocument.Create(command);

        // Assert
        document.Should().NotBeNull().And.BeOfType<ExampleDocument>();
        document.Id.Should().NotBeEmpty();
        document.Created.Should().NotBe(DateTime.MinValue).And.BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        document.Updated.Should().NotBe(DateTime.MinValue).And.Be(document.Created);
        document.Name.Should().NotBeNullOrWhiteSpace().And.Be(command.Name);
        document.ExampleValueObject.Should().NotBeNull().And.BeEquivalentTo(command.ExampleValueObject);
        document.RemoteCode.Should().BeNull();

        domainEvent.Should().NotBeNull().And.BeOfType<ExampleCreated>();
        domainEvent.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        domainEvent.Id.Should().NotBeEmpty().And.Be(document.Id);
        domainEvent.Name.Should().NotBeNullOrWhiteSpace().And.Be(document.Name);
        domainEvent.Description.Should().NotBeNullOrWhiteSpace().And.Be(document.Description);
        domainEvent.ExampleValueObject.Should().NotBeNull().And.BeEquivalentTo(document.ExampleValueObject);
    }
}