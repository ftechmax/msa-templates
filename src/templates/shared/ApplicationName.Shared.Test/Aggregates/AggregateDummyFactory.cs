using System;
using ApplicationName.Shared.Aggregates;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;

namespace ApplicationName.Shared.Test.Aggregates;

internal class AggregateDummyFactory : DummyFactory<IAggregate>
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization());

    protected override IAggregate Create()
    {
        var result = _fixture.Create<IAggregate>();

        A.CallTo(() => result.Id).Returns(_fixture.Create<Guid>());
        A.CallTo(() => result.Created).Returns(DateTime.UtcNow);
        A.CallTo(() => result.Updated).Returns(result.Created);

        return result;
    }
}