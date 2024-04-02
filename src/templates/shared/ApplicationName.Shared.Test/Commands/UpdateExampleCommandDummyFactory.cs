using ApplicationName.Shared.Commands;
using ApplicationName.Shared.Events;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;

namespace ApplicationName.Shared.Test.Commands;

internal class UpdateExampleCommandDummyFactory : DummyFactory<IUpdateExampleCommand>
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization());

    protected override IUpdateExampleCommand Create()
    {
        var result = _fixture.Create<IUpdateExampleCommand>();

        A.CallTo(() => result.CorrelationId).Returns(_fixture.Create<Guid>());
        A.CallTo(() => result.Id).Returns(_fixture.Create<Guid>());
        A.CallTo(() => result.Description).Returns(_fixture.Create<string>());
        A.CallTo(() => result.ExampleValueObject).Returns(A.Dummy<IExampleValueObjectEventData>());

        return result;
    }
}