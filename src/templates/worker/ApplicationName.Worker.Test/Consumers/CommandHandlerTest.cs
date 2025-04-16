using ApplicationName.Shared.Commands;
using ApplicationName.Shared.Events;
using ApplicationName.Worker.Application.Documents;
using ApplicationName.Worker.Application.DomainEvents;
using ApplicationName.Worker.Application.Services;
using ApplicationName.Worker.Consumers;
using ApplicationName.Worker.Contracts.Commands;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using Mapster;
using MapsterMapper;
using MassTransit;
using NUnit.Framework;
using Shouldly;

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

        var mapperConfig = new TypeAdapterConfig();
        mapperConfig.Scan(typeof(MappingProfile).Assembly);
        _mapper = new Mapper(mapperConfig);
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

        capturedCommand.ShouldSatisfyAllConditions(
                i => i.ShouldNotBeNull(),
                i => i.ShouldBe(command));

        capturedEvent.ShouldSatisfyAllConditions(
                i => i.ShouldNotBeNull(),
                i => i.CorrelationId.ShouldSatisfyAllConditions(
                    j => j.ShouldNotBe(Guid.Empty),
                    j => j.ShouldBe(command.CorrelationId)),
                i => i.Id.ShouldSatisfyAllConditions(
                    j => j.ShouldNotBe(Guid.Empty),
                    j => j.ShouldBe(domainEvent.Id)),
                i => i.Name.ShouldSatisfyAllConditions(
                    j => j.ShouldNotBeNullOrWhiteSpace(),
                    j => j.ShouldBe(domainEvent.Name)),
                i => i.Description.ShouldSatisfyAllConditions(
                    j => j.ShouldNotBeNullOrWhiteSpace(),
                    j => j.ShouldBe(domainEvent.Description)),
                i => i.ExampleValueObject.ShouldSatisfyAllConditions(
                    j => j.ShouldNotBeNull(),
                    j => j.Code.ShouldBe(domainEvent.ExampleValueObject.Code),
                    j => j.Value.ShouldBe(domainEvent.ExampleValueObject.Value)));
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

        capturedCommand.ShouldSatisfyAllConditions(
            i => i.ShouldNotBeNull(),
            i => i.ShouldBe(command));

        capturedEvent.ShouldSatisfyAllConditions(
            i => i.ShouldNotBeNull(),
            i => i.CorrelationId.ShouldSatisfyAllConditions(
                j => j.ShouldNotBe(Guid.Empty),
                j => j.ShouldBe(command.CorrelationId)),
            i => i.Id.ShouldSatisfyAllConditions(
                j => j.ShouldNotBe(Guid.Empty),
                j => j.ShouldBe(domainEvent.Id)),
            i => i.Description.ShouldSatisfyAllConditions(
                j => j.ShouldNotBeNullOrWhiteSpace(),
                j => j.ShouldBe(domainEvent.Description)),
            i => i.ExampleValueObject.ShouldSatisfyAllConditions(
                j => j.ShouldNotBeNull(),
                j => j.Code.ShouldBe(domainEvent.ExampleValueObject.Code),
                j => j.Value.ShouldBe(domainEvent.ExampleValueObject.Value)));
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

        capturedCommand.ShouldSatisfyAllConditions(
            i => i.ShouldNotBeNull(),
            i => i.ShouldBe(command));

        capturedEvent.ShouldSatisfyAllConditions(
            i => i.ShouldNotBeNull(),
            i => i.CorrelationId.ShouldSatisfyAllConditions(
                j => j.ShouldNotBe(Guid.Empty),
                j => j.ShouldBe(command.CorrelationId)),
            i => i.Id.ShouldSatisfyAllConditions(
                j => j.ShouldNotBe(Guid.Empty),
                j => j.ShouldBe(domainEvent.Id)),
            i => i.RemoteCode.ShouldSatisfyAllConditions(
                j => j.ShouldNotBe(default),
                j => j.ShouldBe(domainEvent.RemoteCode)));
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