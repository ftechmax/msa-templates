using ApplicationName.Shared.Aggregates;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;

namespace ApplicationName.Shared.Test.Aggregates;

internal class ExampleEntityDummyFactory : DummyFactory<IExampleEntity>
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization());

    protected override IExampleEntity Create()
    {
        var result = _fixture.Create<IExampleEntity>();

        A.CallTo(() => result.Id).Returns(_fixture.Create<Guid>());
        A.CallTo(() => result.Name).Returns(_fixture.Create<string>());
        A.CallTo(() => result.SomeValue).Returns(_fixture.Create<float>());

        return result;
    }
}