using System;
using ApplicationName.Shared.Events;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;

namespace ApplicationName.Shared.Test.Commands;

public class ExampleCreatedEventDummyFactory : DummyFactory<IExampleCreatedEvent>
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization());

    protected override IExampleCreatedEvent Create()
    {
        var result = _fixture.Create<IExampleCreatedEvent>();

        A.CallTo(() => result.CorrelationId).Returns(_fixture.Create<Guid>());
        A.CallTo(() => result.Id).Returns(_fixture.Create<Guid>());
        A.CallTo(() => result.Name).Returns(_fixture.Create<string>());
        A.CallTo(() => result.Description).Returns(_fixture.Create<string>());
        A.CallTo(() => result.ExampleValueObject).Returns(A.Dummy<IExampleValueObjectEventData>());

        return result;
    }
}