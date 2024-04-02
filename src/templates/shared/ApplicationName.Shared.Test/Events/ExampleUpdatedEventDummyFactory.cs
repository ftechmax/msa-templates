using ApplicationName.Shared.Events;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;

namespace ApplicationName.Shared.Test.Commands;

public class ExampleUpdatedEventDummyFactory : DummyFactory<IExampleUpdatedEvent>
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization());

    protected override IExampleUpdatedEvent Create()
    {
        var result = _fixture.Create<IExampleUpdatedEvent>();

        A.CallTo(() => result.CorrelationId).Returns(_fixture.Create<Guid>());
        A.CallTo(() => result.Id).Returns(_fixture.Create<Guid>());
        A.CallTo(() => result.Description).Returns(_fixture.Create<string>());
        A.CallTo(() => result.ExampleValueObject).Returns(A.Dummy<IExampleValueObjectEventData>());

        return result;
    }
}