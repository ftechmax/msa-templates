using ApplicationName.Worker.Consumers;
using ApplicationName.Worker.Contracts.Commands;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using Mapster;
using MapsterMapper;
using MassTransit;
using NUnit.Framework;
using Other.Worker.Contracts.Commands;
using Shouldly;

namespace ApplicationName.Worker.Test.Consumers;

public class ExternalEventHandlerTest
{
    private IFixture _fixture;

    private IMapper _mapper;

    private ISendEndpoint _sendEndPoint;

    private ConsumeContext<ExternalEvent> _context;

    private ExternalEventHandler _subjectUnderTest;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization());

        var mapperConfig = new TypeAdapterConfig();
        mapperConfig.Scan(typeof(MappingProfile).Assembly);
        _mapper = new Mapper(mapperConfig);
        _fixture.Register(() => _mapper);

        EndpointConvention.Map<SetExampleRemoteCodeCommand>(new Uri("queue:test"));

        _sendEndPoint = _fixture.Freeze<ISendEndpoint>();
        _context = _fixture.Freeze<ConsumeContext<ExternalEvent>>();
        A.CallTo(() => _context.GetSendEndpoint(A<Uri>._)).Returns(_sendEndPoint);

        _subjectUnderTest = _fixture.Create<ExternalEventHandler>();
    }

    [Test]
    public async Task Consume_ExternalEvent()
    {
        // Arrange
        var @event = _fixture.Create<ExternalEvent>();
        A.CallTo(() => _context.Message).ReturnsLazily(() => @event);

        var capturedCommand = default(SetExampleRemoteCodeCommand);
        A.CallTo(() => _sendEndPoint.Send(A<SetExampleRemoteCodeCommand>._, A<CancellationToken>._)).Invokes(
            (SetExampleRemoteCodeCommand arg0, CancellationToken _) =>
            {
                capturedCommand = arg0;
            });

        // Act
        await _subjectUnderTest.Consume(_context);

        // Assert
        A.CallTo(() => _sendEndPoint.Send(A<SetExampleRemoteCodeCommand>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();

        capturedCommand.ShouldSatisfyAllConditions(
            i => i.ShouldNotBeNull(),
            i => i.CorrelationId.ShouldBe(@event.CorrelationId),
            i => i.Id.ShouldBe(@event.Id),
            i => i.RemoteCode.ShouldBe(@event.Code));
    }
}