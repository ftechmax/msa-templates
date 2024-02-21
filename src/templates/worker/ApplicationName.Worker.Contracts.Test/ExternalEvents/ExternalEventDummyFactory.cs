using System;
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
        var result = _fixture.Create<IExternalEvent>();

        A.CallTo(() => result.CorrelationId).Returns(_fixture.Create<Guid>());
        A.CallTo(() => result.Id).Returns(_fixture.Create<Guid>());
        A.CallTo(() => result.Code).Returns(_fixture.Create<int>());

        return result;
    }
}