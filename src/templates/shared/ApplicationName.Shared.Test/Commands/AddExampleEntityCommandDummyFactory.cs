using System;
using ApplicationName.Shared.Commands;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;

namespace ApplicationName.Shared.Test.Commands;

public class AddExampleEntityCommandDummyFactory : DummyFactory<IAddExampleEntityCommand>
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization());

    protected override IAddExampleEntityCommand Create()
    {
        var result = _fixture.Create<IAddExampleEntityCommand>();

        A.CallTo(() => result.CorrelationId).Returns(_fixture.Create<Guid>());
        A.CallTo(() => result.Id).Returns(_fixture.Create<Guid>());
        A.CallTo(() => result.Name).Returns(_fixture.Create<string>());
        A.CallTo(() => result.SomeValue).Returns(_fixture.Create<float>());

        return result;
    }
}