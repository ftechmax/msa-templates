using ApplicationName.Api.Application.Repositories;
using ApplicationName.Api.Consumers;
using ApplicationName.Api.Contracts;
using ApplicationName.Api.ServerSentEvents;
using ApplicationName.Shared.Commands;
using ApplicationName.Shared.Events;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using Conveyo;
using NUnit.Framework;

namespace ApplicationName.Api.Test.Consumers
{
    namespace SafetyGenerator.Worker.Test.Consumers
    {
        public class LocalEventHandlerTest
        {
            private IFixture _fixture;

            private IProtoCacheRepository _protoCacheRepository;

            private IServerSentEventsService _events;

            private LocalEventHandler _subjectUnderTest;

            [SetUp]
            public void SetUp()
            {
                _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization());

                _protoCacheRepository = _fixture.Freeze<IProtoCacheRepository>();
                _events = _fixture.Freeze<IServerSentEventsService>();

                _subjectUnderTest = _fixture.Create<LocalEventHandler>();
            }

            [Test]
            public async Task Consume_ExampleCreatedEvent()
            {
                // Arrange
                var @event = _fixture.Create<ExampleCreatedEvent>();

                var context = _fixture.Create<ConsumeContext<ExampleCreatedEvent>>();
                A.CallTo(() => context.Message).ReturnsLazily(() => @event);

                // Act
                await _subjectUnderTest.Consume(context);

                // Assert
                A.CallTo(() => _protoCacheRepository.RemoveAsync(ApplicationConstants.ExampleCollectionCacheKey)).MustHaveHappenedOnceExactly();
            }

            [Test]
            public async Task Consume_ExampleUpdatedEvent()
            {
                // Arrange
                var @event = _fixture.Create<ExampleUpdatedEvent>();

                var context = _fixture.Create<ConsumeContext<ExampleUpdatedEvent>>();
                A.CallTo(() => context.Message).ReturnsLazily(() => @event);

                // Act
                await _subjectUnderTest.Consume(context);

                // Assert
                A.CallTo(() => _protoCacheRepository.RemoveAsync(ApplicationConstants.ExampleCollectionCacheKey)).MustHaveHappenedOnceExactly();
                A.CallTo(() => _protoCacheRepository.RemoveAsync(ApplicationConstants.ExampleDetailsCacheKey(@event.Id))).MustHaveHappenedOnceExactly();
            }

            [Test]
            public async Task Consume_ExampleRemoteCodeSetEvent()
            {
                // Arrange
                var @event = _fixture.Create<ExampleRemoteCodeSetEvent>();

                var context = _fixture.Create<ConsumeContext<ExampleRemoteCodeSetEvent>>();
                A.CallTo(() => context.Message).ReturnsLazily(() => @event);

                // Act
                await _subjectUnderTest.Consume(context);

                // Assert
                A.CallTo(() => _protoCacheRepository.RemoveAsync(ApplicationConstants.ExampleDetailsCacheKey(@event.Id))).MustHaveHappenedOnceExactly();
            }

            [Test]
            public void Handler_DeclaresFaultConsumers()
            {
                Assert.That(_subjectUnderTest, Is.InstanceOf<IConsumer<Fault<CreateExampleCommand>>>());
                Assert.That(_subjectUnderTest, Is.InstanceOf<IConsumer<Fault<UpdateExampleCommand>>>());
            }

            [Test]
            public async Task Consume_CreateExampleFault_PublishesDomainFault()
            {
                // Arrange
                var command = _fixture.Create<CreateExampleCommand>();
                var context = _fixture.Create<ConsumeContext<Fault<CreateExampleCommand>>>();
                A.CallTo(() => context.Message).Returns(CreateFault(command));

                // Act
                await _subjectUnderTest.Consume(context);

                // Assert
                A.CallTo(() => _events.PublishAsync(
                        "DomainFault_CreateExampleCommand",
                        A<object>.That.Matches(i => ((DomainFault)i).CorrelationId == command.CorrelationId),
                        A<CancellationToken>._))
                    .MustHaveHappenedOnceExactly();
            }

            [Test]
            public async Task Consume_UpdateExampleFault_PublishesDomainFault()
            {
                // Arrange
                var command = _fixture.Create<UpdateExampleCommand>();
                var context = _fixture.Create<ConsumeContext<Fault<UpdateExampleCommand>>>();
                A.CallTo(() => context.Message).Returns(CreateFault(command));

                // Act
                await _subjectUnderTest.Consume(context);

                // Assert
                A.CallTo(() => _events.PublishAsync(
                        "DomainFault_UpdateExampleCommand",
                        A<object>.That.Matches(i => ((DomainFault)i).CorrelationId == command.CorrelationId),
                        A<CancellationToken>._))
                    .MustHaveHappenedOnceExactly();
            }

            private Fault<T> CreateFault<T>(T command) where T : class => new()
            {
                FaultId = Guid.NewGuid(),
                FaultedMessageId = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                Exceptions = [ExceptionInfo.From(new InvalidOperationException("failed"))],
                Host = _fixture.Create<HostInfo>(),
                Message = command,
            };
        }
    }
}