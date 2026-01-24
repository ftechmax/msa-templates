using System.Linq.Expressions;
using ApplicationName.Shared.Commands;
using ApplicationName.Worker.Application.Documents;
using ApplicationName.Worker.Application.DomainEvents;
using ApplicationName.Worker.Application.Services;
using ApplicationName.Worker.Contracts.Commands;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using NUnit.Framework;
using Shouldly;

namespace ApplicationName.Worker.Application.Test.Services;

public class ExampleServiceTest
{
    private IFixture _fixture;

    private IDocumentRepository _documentRepository;

    private ExampleService _subjectUnderTest;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization());

        _documentRepository = _fixture.Freeze<IDocumentRepository>();

        _subjectUnderTest = _fixture.Create<ExampleService>();
    }

    [Test]
    public async Task HandleAsync_CreateExampleCommand_With_Valid_Command()
    {
        // Act
        var command = _fixture.Create<CreateExampleCommand>();

        var capturedDocument = default(ExampleDocument);
        A.CallTo(() => _documentRepository.UpsertAsync(A<ExampleDocument>._)).Invokes((ExampleDocument arg1) =>
        {
            capturedDocument = arg1;
        });

        // Arrange
        var result = await _subjectUnderTest.HandleAsync(command);

        // Assert
        A.CallTo(() => _documentRepository.UpsertAsync(A<ExampleDocument>._)).MustHaveHappenedOnceExactly();

        capturedDocument.ShouldNotBeNull();

        result.ShouldSatisfyAllConditions(
            i => i.ShouldNotBeNull(),
            i => i.ShouldBeOfType<ExampleCreated>(),
            i => i.ShouldSatisfyAllConditions(
                j => j.Id.ShouldBe(capturedDocument.Id),
                j => j.Name.ShouldBe(capturedDocument.Name),
                j => j.Description.ShouldBe(capturedDocument.Description),
                j => j.ExampleValueObject.ShouldSatisfyAllConditions(
                    k => k.Code.ShouldBe(capturedDocument.ExampleValueObject.Code),
                    k => k.Value.ShouldBe(capturedDocument.ExampleValueObject.Value))));
    }

    [Test]
    public async Task HandleAsync_CreateExampleCommand_With_Default_Command()
    {
        // Act
        var command = default(CreateExampleCommand);

        // Arrange
        var act = async () => await _subjectUnderTest.HandleAsync(command);

        // Assert
        var exception = await act.ShouldThrowAsync<ArgumentException>();
        exception.Message.ShouldStartWith($"{nameof(command)}");
    }

    [Test]
    public async Task HandleAsync_UpdateExampleCommand_With_Valid_Command()
    {
        // Act
        var existingCommand = _fixture.Create<CreateExampleCommand>();
        var document = new ExampleDocument(existingCommand);
        var existingDocuments = new[] { document };

        var command = _fixture.Create<UpdateExampleCommand>() with { Id = document.Id };

        var foundDocument = default(ExampleDocument);
        A.CallTo(() => _documentRepository.GetAsync(A<Expression<Func<ExampleDocument, bool>>>._))
            .Invokes((Expression<Func<ExampleDocument, bool>> arg0) =>
            {
                foundDocument = existingDocuments.SingleOrDefault(arg0.Compile());
            }).ReturnsLazily(() => foundDocument);

        var capturedDocument = default(ExampleDocument);
        A.CallTo(() => _documentRepository.UpsertAsync(A<ExampleDocument>._))
            .Invokes((ExampleDocument arg1) =>
            {
                capturedDocument = arg1;
            });

        // Arrange
        var result = await _subjectUnderTest.HandleAsync(command);

        // Assert
        A.CallTo(() => _documentRepository.GetAsync(A<Expression<Func<ExampleDocument, bool>>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _documentRepository.UpsertAsync(A<ExampleDocument>._)).MustHaveHappenedOnceExactly();

        result.ShouldSatisfyAllConditions(
            i => i.ShouldNotBeNull(),
            i => i.ShouldBeOfType<ExampleUpdated>(),
            i => i.ShouldSatisfyAllConditions(
                j => j.Id.ShouldBe(capturedDocument.Id),
                j => j.Description.ShouldBe(capturedDocument.Description),
                j => j.ExampleValueObject.ShouldSatisfyAllConditions(
                    k => k.Code.ShouldBe(capturedDocument.ExampleValueObject.Code),
                    k => k.Value.ShouldBe(capturedDocument.ExampleValueObject.Value))));
    }

    [Test]
    public async Task HandleAsync_UpdateExampleCommand_With_Default_Command()
    {
        // Act
        var command = default(UpdateExampleCommand);

        // Arrange
        var act = () => _subjectUnderTest.HandleAsync(command);

        // Assert
        var exception = await act.ShouldThrowAsync<ArgumentException>();
        exception.Message.ShouldStartWith($"{nameof(command)}");
    }

    [Test]
    public async Task HandleAsync_UpdateExampleCommand_With_Invalid_Id()
    {
        // Arrange
        var command = _fixture.Create<UpdateExampleCommand>() with { Id = Guid.Empty };

        // Act
        var act = () => _subjectUnderTest.HandleAsync(command);

        // Assert
        var exception = await act.ShouldThrowAsync<ArgumentException>();
        exception.Message.ShouldMatch($".*{nameof(command.Id)}");

        A.CallTo(() => _documentRepository.UpsertAsync(A<ExampleDocument>._)).MustNotHaveHappened();
    }

    [Test]
    public async Task HandleAsync_SetExampleRemoteCodeCommand_With_Valid_Command()
    {
        // Act
        var existingCommand = _fixture.Create<CreateExampleCommand>();
        var document = new ExampleDocument(existingCommand);
        var existingDocuments = new[] { document };

        var command = _fixture.Create<SetExampleRemoteCodeCommand>() with { Id = document.Id };

        var foundDocument = default(ExampleDocument);
        A.CallTo(() => _documentRepository.GetAsync(A<Expression<Func<ExampleDocument, bool>>>._))
            .Invokes((Expression<Func<ExampleDocument, bool>> arg0) =>
            {
                foundDocument = existingDocuments.SingleOrDefault(arg0.Compile());
            }).ReturnsLazily(() => foundDocument);

        var capturedDocument = default(ExampleDocument);
        A.CallTo(() => _documentRepository.UpsertAsync(A<ExampleDocument>._))
            .Invokes((ExampleDocument arg1) =>
            {
                capturedDocument = arg1;
            });

        // Arrange
        var result = await _subjectUnderTest.HandleAsync(command);

        // Assert
        A.CallTo(() => _documentRepository.GetAsync(A<Expression<Func<ExampleDocument, bool>>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _documentRepository.UpsertAsync(A<ExampleDocument>._)).MustHaveHappenedOnceExactly();

        result.ShouldSatisfyAllConditions(
            i => i.ShouldNotBeNull(),
            i => i.ShouldBeOfType<ExampleRemoteCodeSet>(),
            i => i.ShouldSatisfyAllConditions(
                j => j.Id.ShouldBe(capturedDocument.Id),
                j => j.Timestamp.ShouldBe(DateTime.UtcNow, TimeSpan.FromSeconds(1)),
                j => j.RemoteCode.ShouldBe(capturedDocument.RemoteCode.Value)));
    }

    [Test]
    public async Task HandleAsync_SetExampleRemoteCodeCommand_With_Default_Command()
    {
        // Act
        var command = default(SetExampleRemoteCodeCommand);

        // Arrange
        var act = () => _subjectUnderTest.HandleAsync(command);

        // Assert
        var exception = await act.ShouldThrowAsync<ArgumentException>();
        exception.Message.ShouldStartWith($"{nameof(command)}");
    }

    [Test]
    public async Task HandleAsync_SetExampleRemoteCodeCommand_With_Invalid_Id()
    {
        // Arrange
        var command = _fixture.Create<SetExampleRemoteCodeCommand>() with { Id = Guid.Empty };

        // Act
        var act = () => _subjectUnderTest.HandleAsync(command);

        // Assert
        var exception = await act.ShouldThrowAsync<ArgumentException>();
        exception.Message.ShouldMatch($".*{nameof(command.Id)}");

        A.CallTo(() => _documentRepository.UpsertAsync(A<ExampleDocument>._)).MustNotHaveHappened();
    }
}