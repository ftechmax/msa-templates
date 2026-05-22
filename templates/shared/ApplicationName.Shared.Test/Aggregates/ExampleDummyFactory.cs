using ApplicationName.Shared.Aggregates;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;

namespace ApplicationName.Shared.Test.Aggregates;

internal class ExampleDummyFactory : DummyFactory<IExample>
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization());

    protected override IExample Create()
    {
        var result = _fixture.Create<IExample>();

        A.CallTo(() => result.Id).Returns(_fixture.Create<Guid>());
        A.CallTo(() => result.Created).Returns(_fixture.Create<DateTime>());
        A.CallTo(() => result.Updated).Returns(result.Created);
        A.CallTo(() => result.Name).Returns(_fixture.Create<string>());
        A.CallTo(() => result.Description).Returns(_fixture.Create<string>());
        A.CallTo(() => result.ExampleValueObject).Returns(A.Dummy<IExampleValueObject>());

        return result;
    }
}