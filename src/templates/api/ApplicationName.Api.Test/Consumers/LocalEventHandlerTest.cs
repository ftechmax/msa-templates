using ApplicationName.Api.Application.Repositories;
using ApplicationName.Api.Consumers;
using ApplicationName.Api.Contracts;
using ApplicationName.Shared.Events;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using MassTransit;
using NUnit.Framework;

namespace ApplicationName.Api.Test.Consumers
{
    namespace SafetyGenerator.Worker.Test.Consumers
    {
        public class LocalEventHandlerTest
        {
            private IFixture _fixture;

            private IProtoCacheRepository _protoCacheRepository;

            private LocalEventHandler _subjectUnderTest;

            [SetUp]
            public void SetUp()
            {
                _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization());

                _protoCacheRepository = _fixture.Freeze<IProtoCacheRepository>();

                _subjectUnderTest = _fixture.Create<LocalEventHandler>();
            }

            [Test]
            public async Task Consume_IExampleCreatedEvent()
            {
                // Arrange
                var @event = A.Dummy<IExampleCreatedEvent>();

                var context = _fixture.Create<ConsumeContext<IExampleCreatedEvent>>();
                A.CallTo(() => context.Message).ReturnsLazily(() => @event);

                // Act
                await _subjectUnderTest.Consume(context);

                // Assert
                A.CallTo(() => _protoCacheRepository.RemoveAsync(ApplicationConstants.ExampleCollectionCacheKey)).MustHaveHappenedOnceExactly();
            }

            [Test]
            public async Task Consume_IExampleUpdatedEvent()
            {
                // Arrange
                var @event = A.Dummy<IExampleUpdatedEvent>();

                var context = _fixture.Create<ConsumeContext<IExampleUpdatedEvent>>();
                A.CallTo(() => context.Message).ReturnsLazily(() => @event);

                // Act
                await _subjectUnderTest.Consume(context);

                // Assert
                A.CallTo(() => _protoCacheRepository.RemoveAsync(ApplicationConstants.ExampleCollectionCacheKey)).MustHaveHappenedOnceExactly();
                A.CallTo(() => _protoCacheRepository.RemoveAsync(ApplicationConstants.ExampleDetailsCacheKey(@event.Id))).MustHaveHappenedOnceExactly();
            }

            [Test]
            public async Task Consume_IExampleEntityAddedEvent()
            {
                // Arrange
                var @event = A.Dummy<IExampleEntityAddedEvent>();

                var context = _fixture.Create<ConsumeContext<IExampleEntityAddedEvent>>();
                A.CallTo(() => context.Message).ReturnsLazily(() => @event);

                // Act
                await _subjectUnderTest.Consume(context);

                // Assert
                A.CallTo(() => _protoCacheRepository.RemoveAsync(ApplicationConstants.ExampleDetailsCacheKey(@event.Id))).MustHaveHappenedOnceExactly();
            }

            [Test]
            public async Task Consume_IExampleEntityUpdatedEvent()
            {
                // Arrange
                var @event = A.Dummy<IExampleEntityUpdatedEvent>();

                var context = _fixture.Create<ConsumeContext<IExampleEntityUpdatedEvent>>();
                A.CallTo(() => context.Message).ReturnsLazily(() => @event);

                // Act
                await _subjectUnderTest.Consume(context);

                // Assert
                A.CallTo(() => _protoCacheRepository.RemoveAsync(ApplicationConstants.ExampleDetailsCacheKey(@event.Id))).MustHaveHappenedOnceExactly();
            }

            [Test]
            public async Task Consume_IExampleRemoteCodeSetEvent()
            {
                // Arrange
                var @event = A.Dummy<IExampleRemoteCodeSetEvent>();

                var context = _fixture.Create<ConsumeContext<IExampleRemoteCodeSetEvent>>();
                A.CallTo(() => context.Message).ReturnsLazily(() => @event);

                // Act
                await _subjectUnderTest.Consume(context);

                // Assert
                A.CallTo(() => _protoCacheRepository.RemoveAsync(ApplicationConstants.ExampleDetailsCacheKey(@event.Id))).MustHaveHappenedOnceExactly();
            }
        }
    }
}