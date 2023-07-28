using ApplicationName.Worker.Contracts.Specifications;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;

namespace ApplicationName.Worker.Contracts.Test.Specifications;

public class ExampleSpecificationDummyFactory : DummyFactory<IExampleSpecification>
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization());

    protected override IExampleSpecification Create()
    {
        var spec = _fixture.Create<IExampleSpecification>();

        A.CallTo(() => spec.Name).Returns(_fixture.Create<string>());

        return spec;
    }
}