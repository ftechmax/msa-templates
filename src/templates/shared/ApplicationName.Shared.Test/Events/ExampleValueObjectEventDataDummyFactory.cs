using ApplicationName.Shared.Events;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;

namespace ApplicationName.Shared.Test.Commands;

public class ExampleValueObjectEventDataDummyFactory : DummyFactory<IExampleValueObjectEventData>
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization());

    protected override IExampleValueObjectEventData Create()
    {
        var result = _fixture.Create<IExampleValueObjectEventData>();

        A.CallTo(() => result.Code).Returns(_fixture.Create<string>());
        A.CallTo(() => result.Value).Returns(_fixture.Create<double>());

        return result;
    }
}