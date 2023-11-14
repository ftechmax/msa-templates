using System;
using ApplicationName.Worker.Contracts.Commands;
using ApplicationName.Worker.Contracts.Events;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;

namespace ApplicationName.Worker.Contracts.Test.Commands;

public class ExampleCommandDummyFactory : DummyFactory<ICreateExampleCommand>
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization());

    protected override ICreateExampleCommand Create()
    {
        var result = _fixture.Create<ICreateExampleCommand>();

        A.CallTo(() => result.CorrelationId).Returns(_fixture.Create<Guid>());
        A.CallTo(() => result.Name).Returns(_fixture.Create<string>());
        A.CallTo(() => result.Description).Returns(_fixture.Create<string>());
        A.CallTo(() => result.ExampleValueObject).Returns(A.Dummy<IExampleValueObjectEventData>());

        return result;
    }
}