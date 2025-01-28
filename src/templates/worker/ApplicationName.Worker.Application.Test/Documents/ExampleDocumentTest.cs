using ApplicationName.Shared.Commands;
using ApplicationName.Worker.Application.Documents;
using ApplicationName.Worker.Application.DomainEvents;
using ApplicationName.Worker.Contracts.Commands;
using ApplicationName.Worker.Contracts.Test;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using NUnit.Framework;
using Shouldly;

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
        result.ShouldSatisfyAllConditions(
            i => i.Id.ShouldNotBe(Guid.Empty),
            i => i.Created.ShouldSatisfyAllConditions(
                j => j.ShouldNotBe(DateTime.MinValue),
                j => j.ShouldBe(DateTime.UtcNow, TimeSpan.FromSeconds(1))),
            i => i.Updated.ShouldSatisfyAllConditions(
                j => j.ShouldNotBe(DateTime.MinValue),
                j => j.ShouldBe(result.Created)),
            i => i.Name.ShouldSatisfyAllConditions(
                j => j.ShouldNotBeNullOrWhiteSpace(),
                j => j.ShouldBe(command.Name)),
            i => i.ExampleValueObject.ShouldSatisfyAllConditions(
                j => j.ShouldNotBeNull(),
                j => j.Code.ShouldBe(command.ExampleValueObject.Code),
                j => j.Value.ShouldBe(command.ExampleValueObject.Value)),
            i => i.RemoteCode.ShouldBeNull());
    }

    [Test]
    public void Constructor_With_Invalid_Command()
    {
        // Arrange
        var command = default(CreateExampleCommand);

        // Act
        var act = () => new ExampleDocument(command);

        // Assert
        act.ShouldThrow<ArgumentException>().Message.ShouldStartWith($"{nameof(command)}");
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
        act.ShouldThrow<ArgumentException>().Message.ShouldStartWith($"{nameof(command.Name)}");
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
        act.ShouldThrow<ArgumentException>().Message.ShouldStartWith($"{nameof(command.Description)}");
    }

    [Test]
    public void Constructor_With_Invalid_ExampleValueObject()
    {
        // Arrange
        var command = _fixture.Create<CreateExampleCommand>() with { ExampleValueObject = default };

        // Act
        var act = () => new ExampleDocument(command);

        // Assert
        act.ShouldThrow<ArgumentException>().Message.ShouldStartWith($"{nameof(command.ExampleValueObject)}");
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
        result.ShouldSatisfyAllConditions(
            i => i.ShouldNotBeNull(),
            i => i.ShouldBeOfType<ExampleUpdated>(),
            i => i.Timestamp.ShouldBe(DateTime.UtcNow, TimeSpan.FromSeconds(1)),
            i => i.Id.ShouldNotBe(Guid.Empty),
            i => i.Id.ShouldBe(document.Id),
            i => i.Description.ShouldNotBeNullOrWhiteSpace(),
            i => i.Description.ShouldBe(command.Description),
            i => i.ExampleValueObject.ShouldSatisfyAllConditions(
                j => j.ShouldNotBeNull(),
                j => j.Code.ShouldBe(command.ExampleValueObject.Code),
                j => j.Value.ShouldBe(command.ExampleValueObject.Value)));

        document.Updated.ShouldBe(DateTime.UtcNow, TimeSpan.FromSeconds(1));
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
        act.ShouldThrow<ArgumentException>().Message.ShouldStartWith($"{nameof(command)}");
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
        act.ShouldThrow<ArgumentException>().Message.ShouldStartWith($"{nameof(command.Description)}");
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
        act.ShouldThrow<ArgumentException>().Message.ShouldStartWith($"{nameof(command.ExampleValueObject)}");
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
        result.ShouldSatisfyAllConditions(
            i => i.ShouldNotBeNull(),
            i => i.ShouldBeOfType<ExampleRemoteCodeSet>(),
            i => i.Timestamp.ShouldBe(DateTime.UtcNow, TimeSpan.FromSeconds(1)),
            i => i.Id.ShouldNotBe(Guid.Empty),
            i => i.Id.ShouldBe(document.Id),
            i => i.RemoteCode.ShouldBe(command.RemoteCode));

        document.Updated.ShouldBe(DateTime.UtcNow, TimeSpan.FromSeconds(1));
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
        act.ShouldThrow<ArgumentException>().Message.ShouldStartWith($"{nameof(command)}");
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
        act.ShouldThrow<ArgumentException>().Message.ShouldStartWith($"{nameof(command.RemoteCode)}");
    }

    [Test]
    public void Create()
    {
        // Arrange
        var command = _fixture.Create<CreateExampleCommand>();

        // Act
        var (document, domainEvent) = ExampleDocument.Create(command);

        // Assert
        document.ShouldSatisfyAllConditions(
            i => i.ShouldNotBeNull(),
            i => i.ShouldBeOfType<ExampleDocument>(),
            i => i.Id.ShouldNotBe(Guid.Empty),
            i => i.Created.ShouldSatisfyAllConditions(
                j => j.ShouldNotBe(DateTime.MinValue),
                j => j.ShouldBe(DateTime.UtcNow, TimeSpan.FromSeconds(1))),
            i => i.Updated.ShouldSatisfyAllConditions(
                j => j.ShouldNotBe(DateTime.MinValue),
                j => j.ShouldBe(i.Created)),
            i => i.Name.ShouldNotBeNullOrWhiteSpace(),
            i => i.Name.ShouldBe(command.Name),
            i => i.ExampleValueObject.ShouldSatisfyAllConditions(
                j => j.ShouldNotBeNull(),
                j => j.Code.ShouldBe(command.ExampleValueObject.Code),
                j => j.Value.ShouldBe(command.ExampleValueObject.Value)),
            i => i.RemoteCode.ShouldBeNull());

        domainEvent.ShouldSatisfyAllConditions(
            i => i.ShouldNotBeNull(),
            i => i.ShouldBeOfType<ExampleCreated>(),
            i => i.Timestamp.ShouldBe(DateTime.UtcNow, TimeSpan.FromSeconds(1)),
            i => i.Id.ShouldNotBe(Guid.Empty),
            i => i.Id.ShouldBe(document.Id),
            i => i.Name.ShouldNotBeNullOrWhiteSpace(),
            i => i.Name.ShouldBe(document.Name),
            i => i.Description.ShouldNotBeNullOrWhiteSpace(),
            i => i.Description.ShouldBe(document.Description),
            i => i.ExampleValueObject.ShouldSatisfyAllConditions(
                j => j.ShouldNotBeNull(),
                j => j.Code.ShouldBe(document.ExampleValueObject.Code),
                j => j.Value.ShouldBe(document.ExampleValueObject.Value)));
    }
}