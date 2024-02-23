using System;
using ApplicationName.Shared.Commands;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;

namespace ApplicationName.Shared.Test.Commands;

internal class UpdateExampleEntityCommandDummyFactory : DummyFactory<IUpdateExampleEntityCommand>
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization());

    protected override IUpdateExampleEntityCommand Create()
    {
        var result = _fixture.Create<IUpdateExampleEntityCommand>();

        A.CallTo(() => result.CorrelationId).Returns(_fixture.Create<Guid>());
        A.CallTo(() => result.Id).Returns(_fixture.Create<Guid>());
        A.CallTo(() => result.EntityId).Returns(_fixture.Create<Guid>());
        A.CallTo(() => result.SomeValue).Returns(_fixture.Create<float>());

        return result;
    }
}