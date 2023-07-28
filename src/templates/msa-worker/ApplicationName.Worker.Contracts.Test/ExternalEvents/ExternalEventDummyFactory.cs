using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using Other.Worker.Contracts.Commands;

namespace ApplicationName.Worker.Contracts.Test.ExternalEvents;

public class ExternalEventDummyFactory : DummyFactory<IExternalEvent>
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization());

    protected override IExternalEvent Create()
    {
        var @event = _fixture.Create<IExternalEvent>();

        A.CallTo(() => @event.Name).Returns(_fixture.Create<string>());

        return @event;
    }
}