using System.Linq.Expressions;
using ApplicationName.Shared.Commands;
using ApplicationName.Worker.Application.Documents;
using ApplicationName.Worker.Application.DomainEvents;
using ApplicationName.Worker.Application.Services;
using ApplicationName.Worker.Contracts.Commands;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;

namespace ApplicationName.Worker.Application.Test.Services;

public class ApplicationServiceTest
{
    private IFixture _fixture;

    private IDocumentRepository _documentRepository;

    private ApplicationService _subjectUnderTest;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization());

        _documentRepository = _fixture.Freeze<IDocumentRepository>();

        _subjectUnderTest = _fixture.Create<ApplicationService>();
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

        capturedDocument.Should().NotBeNull();
        result.Should().NotBeNull().And.BeOfType<ExampleCreated>().And.BeEquivalentTo(capturedDocument, opts => opts.ExcludingMissingMembers());
    }

    [Test]
    public async Task HandleAsync_CreateExampleCommand_With_Default_Command()
    {
        // Act
        var command = default(CreateExampleCommand);

        // Arrange
        Func<Task> act = () => _subjectUnderTest.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage($"{nameof(command)}*");
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

        result.Should()
            .NotBeNull()
            .And.BeOfType<ExampleUpdated>()
            .And.BeEquivalentTo(capturedDocument, opts => opts.ExcludingMissingMembers());
    }

    [Test]
    public async Task HandleAsync_UpdateExampleCommand_With_Default_Command()
    {
        // Act
        var command = default(UpdateExampleCommand);

        // Arrange
        Func<Task> act = () => _subjectUnderTest.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage($"{nameof(command)}*");
    }

    [Test]
    public async Task HandleAsync_UpdateExampleCommand_With_Invalid_Id()
    {
        // Arrange
        var command = _fixture.Create<UpdateExampleCommand>() with { Id = Guid.Empty };

        // Act
        var act = () => _subjectUnderTest.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage($"{nameof(command.Id)}*");
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

        result.Should()
            .NotBeNull()
            .And.BeOfType<ExampleRemoteCodeSet>()
            .And.BeEquivalentTo(capturedDocument, opts => opts.ExcludingMissingMembers());
    }

    [Test]
    public async Task HandleAsync_SetExampleRemoteCodeCommand_With_Default_Command()
    {
        // Act
        var command = default(SetExampleRemoteCodeCommand);

        // Arrange
        Func<Task> act = () => _subjectUnderTest.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage($"{nameof(command)}*");
    }

    [Test]
    public async Task HandleAsync_SetExampleRemoteCodeCommand_With_Invalid_Id()
    {
        // Arrange
        var command = _fixture.Create<SetExampleRemoteCodeCommand>() with { Id = Guid.Empty };

        // Act
        var act = () => _subjectUnderTest.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage($"{nameof(command.Id)}*");
        A.CallTo(() => _documentRepository.UpsertAsync(A<ExampleDocument>._)).MustNotHaveHappened();
    }
}