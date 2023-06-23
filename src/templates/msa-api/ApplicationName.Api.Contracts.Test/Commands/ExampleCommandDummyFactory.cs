using ApplicationName.Api.Contracts.Commands;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;

namespace ApplicationName.Api.Contracts.Test.Commands;

public class ExampleCommandDummyFactory : DummyFactory<IExampleCommand>
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization());

    protected override IExampleCommand Create()
    {
        var command = _fixture.Create<IExampleCommand>();

        A.CallTo(() => command.Name).Returns(_fixture.Create<string>());

        return command;
    }
}