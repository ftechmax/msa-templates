using ApplicationName.Shared.Commands;
using ApplicationName.Shared.Events;
using ApplicationName.Worker.Application.Documents;
using ApplicationName.Worker.Application.DomainEvents;
using ApplicationName.Worker.Application.Services;
using ApplicationName.Worker.Consumers;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using AutoMapper;
using FakeItEasy;
using FluentAssertions;
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
        var command = A.Dummy<CreateExampleCommand>();
        var context = _fixture.Create<ConsumeContext<CreateExampleCommand>>();
        A.CallTo(() => context.Message).Returns(command);

        var documentSpec = A.Dummy<CreateExampleCommand>();
        var document = new ExampleDocument(documentSpec);
        var domainEvent = new ExampleCreated(document);

        var capturedCommand = default(CreateExampleCommand);
        A.CallTo(() => _applicationService.HandleAsync(context.Message))
            .Invokes((CreateExampleCommand arg0) =>
            {
                capturedCommand = arg0;
            })
            .ReturnsLazily(() => domainEvent);

        var capturedEvent = default(ExampleCreatedEvent);
        A.CallTo(() => context.Publish(A<ExampleCreatedEvent>._, A<CancellationToken>._)).Invokes((ExampleCreatedEvent arg0, CancellationToken _) =>
        {
            capturedEvent = arg0;
        });

        // Act
        await _subjectUnderTest.Consume(context);

        // Assert
        A.CallTo(() => _applicationService.HandleAsync(context.Message)).MustHaveHappenedOnceExactly();
        A.CallTo(() => context.Publish(A<ExampleCreatedEvent>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        capturedEvent.Should().NotBeNull();
        capturedCommand.Should().NotBeNull().And.Be(command);
    }
}