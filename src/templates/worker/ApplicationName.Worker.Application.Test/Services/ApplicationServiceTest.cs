using ApplicationName.Shared.Commands;
using ApplicationName.Worker.Application.Documents;
using ApplicationName.Worker.Application.DomainEvents;
using ApplicationName.Worker.Application.Services;
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
    public async Task HandleAsync_ICreateExampleCommand_With_Valid_Command()
    {
        // Act
        var command = A.Dummy<CreateExampleCommand>();

        var capturedDocument = default(ExampleDocument);
        A.CallTo(() => _documentRepository.UpsertAsync(A<ExampleDocument>._)).Invokes((ExampleDocument arg1) =>
        {
            capturedDocument = arg1;
        });

        // Arrange
        var result = await _subjectUnderTest.HandleAsync(command);

        // Assert
        A.CallTo(() => _documentRepository.UpsertAsync(A<ExampleDocument>._)).MustHaveHappenedOnceExactly();

        capturedDocument.Id.Should().NotBeEmpty();
        capturedDocument.Name.Should().NotBeNullOrWhiteSpace().And.Be(command.Name);
        capturedDocument.Description.Should().NotBeNullOrWhiteSpace().And.Be(command.Description);
        capturedDocument.ExampleValueObject.Should().NotBeNull().And.BeEquivalentTo(command.ExampleValueObject);
        capturedDocument.RemoteCode.Should().BeNull();

        result.Should().NotBeNull().And.BeOfType<ExampleCreated>();
        result.Id.Should().NotBeEmpty().And.Be(capturedDocument.Id);
        result.Description.Should().NotBeNullOrWhiteSpace().And.Be(capturedDocument.Description);
        result.ExampleValueObject.Should().NotBeNull().And.BeEquivalentTo(capturedDocument.ExampleValueObject);
    }

    [Test]
    public async Task HandleAsync_ICreateExampleCommand_With_Default_Command()
    {
        // Act
        var command = default(CreateExampleCommand);

        // Arrange
        Func<Task> act = () => _subjectUnderTest.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage($"{nameof(command)}*");
    }
}