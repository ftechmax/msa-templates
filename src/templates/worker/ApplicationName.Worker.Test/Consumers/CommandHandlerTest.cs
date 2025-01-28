using ApplicationName.Shared.Commands;
using ApplicationName.Shared.Events;
using ApplicationName.Worker.Application.Documents;
using ApplicationName.Worker.Application.DomainEvents;
using ApplicationName.Worker.Application.Services;
using ApplicationName.Worker.Consumers;
using ApplicationName.Worker.Contracts.Commands;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using AutoMapper;
using FakeItEasy;
using Shouldly;
using MassTransit;
using NUnit.Framework;

namespace ApplicationName.Worker.Test.Consumers;

public class CommandHandlerTest
{
    private IFixture _fixture;

    private IMapper _mapper;

    private IApplicationService _applicationService;

    private CommandHandler _subjectUnderTest;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization());

        _mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper();
        _fixture.Register(() => _mapper);

        _applicationService = _fixture.Freeze<IApplicationService>();

        _subjectUnderTest = _fixture.Create<CommandHandler>();
    }

    [Test]
    public async Task Consume_CreateExampleCommand()
    {
        // Arrange
        var command = _fixture.Create<CreateExampleCommand>();
        var context = _fixture.Create<ConsumeContext<CreateExampleCommand>>();
        A.CallTo(() => context.Message).Returns(command);

        var existingCommand = _fixture.Create<CreateExampleCommand>();
        var document = new ExampleDocument(existingCommand);
        var domainEvent = new ExampleCreated(document);

        var capturedCommand = default(CreateExampleCommand);
        A.CallTo(() => _applicationService.HandleAsync(context.Message))
            .Invokes((CreateExampleCommand arg0) =>
            {
                capturedCommand = arg0;
            })
            .ReturnsLazily(() => domainEvent);

        var capturedEvent = default(ExampleCreatedEvent);
        A.CallTo(() => context.Publish(A<ExampleCreatedEvent>._, A<CancellationToken>._))
            .Invokes((ExampleCreatedEvent arg0, CancellationToken _) =>
            {
                capturedEvent = arg0;
            });

        // Act
        await _subjectUnderTest.Consume(context);

        // Assert
        A.CallTo(() => _applicationService.HandleAsync(context.Message)).MustHaveHappenedOnceExactly();
        A.CallTo(() => context.Publish(A<ExampleCreatedEvent>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();

        capturedCommand.Should().NotBeNull().And.Be(command);

        capturedEvent.Should().NotBeNull();
        capturedEvent.CorrelationId.Should().NotBeEmpty().And.Be(command.CorrelationId);
        capturedEvent.Id.Should().NotBeEmpty().And.Be(domainEvent.Id);
        capturedEvent.Name.Should().NotBeNullOrWhiteSpace().And.Be(domainEvent.Name);
        capturedEvent.Description.Should().NotBeNullOrWhiteSpace().And.Be(domainEvent.Description);
        capturedEvent.ExampleValueObject.Should().NotBeNull().And.BeEquivalentTo(domainEvent.ExampleValueObject);
    }

    [Test]
    public async Task Consume_CreateExampleCommand_Without_DomainEvent()
    {
        // Arrange
        var command = _fixture.Create<CreateExampleCommand>();
        var context = _fixture.Create<ConsumeContext<CreateExampleCommand>>();
        A.CallTo(() => context.Message).Returns(command);

        A.CallTo(() => _applicationService.HandleAsync(context.Message)).Returns(default(ExampleCreated));

        // Act
        await _subjectUnderTest.Consume(context);

        // Assert
        A.CallTo(() => _applicationService.HandleAsync(context.Message)).MustHaveHappenedOnceExactly();
        A.CallTo(() => context.Publish(A<ExampleCreatedEvent>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Test]
    public async Task Consume_UpdateExampleCommand()
    {
        // Arrange
        var existingCommand = _fixture.Create<CreateExampleCommand>();
        var document = new ExampleDocument(existingCommand);
        var domainEvent = new ExampleUpdated(document);

        var command = _fixture.Create<UpdateExampleCommand>() with { Id = document.Id };
        var context = _fixture.Create<ConsumeContext<UpdateExampleCommand>>();
        A.CallTo(() => context.Message).Returns(command);

        var capturedCommand = default(UpdateExampleCommand);
        A.CallTo(() => _applicationService.HandleAsync(context.Message))
            .Invokes((UpdateExampleCommand arg0) =>
            {
                capturedCommand = arg0;
            })
            .ReturnsLazily(() => domainEvent);

        var capturedEvent = default(ExampleUpdatedEvent);
        A.CallTo(() => context.Publish(A<ExampleUpdatedEvent>._, A<CancellationToken>._))
            .Invokes((ExampleUpdatedEvent arg0, CancellationToken _) =>
            {
                capturedEvent = arg0;
            });

        // Act
        await _subjectUnderTest.Consume(context);

        // Assert
        A.CallTo(() => _applicationService.HandleAsync(context.Message)).MustHaveHappenedOnceExactly();
        A.CallTo(() => context.Publish(A<ExampleUpdatedEvent>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();

        capturedCommand.Should().NotBeNull().And.Be(command);

        capturedEvent.Should().NotBeNull();
        capturedEvent.CorrelationId.Should().NotBeEmpty().And.Be(command.CorrelationId);
        capturedEvent.Id.Should().NotBeEmpty().And.Be(domainEvent.Id);
        capturedEvent.Description.Should().NotBeNullOrWhiteSpace().And.Be(domainEvent.Description);
        capturedEvent.ExampleValueObject.Should().NotBeNull().And.BeEquivalentTo(domainEvent.ExampleValueObject);
    }

    [Test]
    public async Task Consume_UpdateExampleCommand_Without_DomainEvent()
    {
        // Arrange
        var command = _fixture.Create<UpdateExampleCommand>();
        var context = _fixture.Create<ConsumeContext<UpdateExampleCommand>>();
        A.CallTo(() => context.Message).Returns(command);

        A.CallTo(() => _applicationService.HandleAsync(context.Message)).Returns(default(ExampleUpdated));

        // Act
        await _subjectUnderTest.Consume(context);

        // Assert
        A.CallTo(() => _applicationService.HandleAsync(context.Message)).MustHaveHappenedOnceExactly();
        A.CallTo(() => context.Publish(A<ExampleUpdatedEvent>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Test]
    public async Task Consume_SetExampleRemoteCodeCommand()
    {
        // Arrange
        var id = _fixture.Create<Guid>();
        var remoteCode = _fixture.Create<int>();
        var domainEvent = new ExampleRemoteCodeSet(id, remoteCode);

        var command = _fixture.Create<SetExampleRemoteCodeCommand>() with { Id = id };
        var context = _fixture.Create<ConsumeContext<SetExampleRemoteCodeCommand>>();
        A.CallTo(() => context.Message).Returns(command);

        var capturedCommand = default(SetExampleRemoteCodeCommand);
        A.CallTo(() => _applicationService.HandleAsync(context.Message))
            .Invokes((SetExampleRemoteCodeCommand arg0) =>
            {
                capturedCommand = arg0;
            })
            .ReturnsLazily(() => domainEvent);

        var capturedEvent = default(ExampleRemoteCodeSetEvent);
        A.CallTo(() => context.Publish(A<ExampleRemoteCodeSetEvent>._, A<CancellationToken>._))
            .Invokes((ExampleRemoteCodeSetEvent arg0, CancellationToken _) =>
            {
                capturedEvent = arg0;
            });

        // Act
        await _subjectUnderTest.Consume(context);

        // Assert
        A.CallTo(() => _applicationService.HandleAsync(context.Message)).MustHaveHappenedOnceExactly();
        A.CallTo(() => context.Publish(A<ExampleRemoteCodeSetEvent>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();

        capturedCommand.Should().NotBeNull().And.Be(command);

        capturedEvent.Should().NotBeNull();
        capturedEvent.CorrelationId.Should().NotBeEmpty().And.Be(command.CorrelationId);
        capturedEvent.Id.Should().NotBeEmpty().And.Be(domainEvent.Id);
        capturedEvent.RemoteCode.Should().Be(domainEvent.RemoteCode);
    }

    [Test]
    public async Task Consume_SetExampleRemoteCodeCommand_Without_DomainEvent()
    {
        // Arrange
        var command = _fixture.Create<SetExampleRemoteCodeCommand>();
        var context = _fixture.Create<ConsumeContext<SetExampleRemoteCodeCommand>>();
        A.CallTo(() => context.Message).Returns(command);

        A.CallTo(() => _applicationService.HandleAsync(context.Message)).Returns(default(ExampleRemoteCodeSet));

        // Act
        await _subjectUnderTest.Consume(context);

        // Assert
        A.CallTo(() => _applicationService.HandleAsync(context.Message)).MustHaveHappenedOnceExactly();
        A.CallTo(() => context.Publish(A<ExampleRemoteCodeSetEvent>._, A<CancellationToken>._)).MustNotHaveHappened();
    }
}