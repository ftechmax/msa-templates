using ApplicationName.Worker.Contracts.Commands;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;

namespace ApplicationName.Api.Contracts.Test.Commands;

public class ExampleCommandDummyFactory : DummyFactory<ICreateExampleCommand>
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization());

    protected override ICreateExampleCommand Create()
    {
        var command = _fixture.Create<ICreateExampleCommand>();

        A.CallTo(() => command.Name).Returns(_fixture.Create<string>());

        return command;
    }
}