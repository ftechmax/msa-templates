using ApplicationName.Api.Contracts.Events;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;

namespace ApplicationName.Api.Contracts.Test.Events;

public class ExampleEventDummyFactory : DummyFactory<IExampleEvent>
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization());

    protected override IExampleEvent Create()
    {
        var @event = _fixture.Create<IExampleEvent>();

        A.CallTo(() => @event.Name).Returns(_fixture.Create<string>());

        return @event;
    }
}