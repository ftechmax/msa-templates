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
            public async Task Consume_ExampleCreatedEvent()
            {
                // Arrange
                var @event = A.Dummy<ExampleCreatedEvent>();

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
                var @event = A.Dummy<ExampleUpdatedEvent>();

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
                var @event = A.Dummy<ExampleRemoteCodeSetEvent>();

                var context = _fixture.Create<ConsumeContext<ExampleRemoteCodeSetEvent>>();
                A.CallTo(() => context.Message).ReturnsLazily(() => @event);

                // Act
                await _subjectUnderTest.Consume(context);

                // Assert
                A.CallTo(() => _protoCacheRepository.RemoveAsync(ApplicationConstants.ExampleDetailsCacheKey(@event.Id))).MustHaveHappenedOnceExactly();
            }
        }
    }
}