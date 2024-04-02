using ApplicationName.Shared.Aggregates;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;

namespace ApplicationName.Shared.Test.Aggregates;

internal class ExampleValueObjectDummyFactory : DummyFactory<IExampleValueObject>
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization());

    protected override IExampleValueObject Create()
    {
        var result = _fixture.Create<IExampleValueObject>();

        A.CallTo(() => result.Code).Returns(_fixture.Create<string>());
        A.CallTo(() => result.Value).Returns(_fixture.Create<double>());

        return result;
    }
}