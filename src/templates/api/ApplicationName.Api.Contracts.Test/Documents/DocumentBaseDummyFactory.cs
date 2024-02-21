using ApplicationName.Api.Contracts.Documents;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using System;

namespace ApplicationName.Api.Contracts.Test.Documents;

public class DocumentBaseDummyFactory : DummyFactory<IDocumentBase>
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization());

    protected override IDocumentBase Create()
    {
        var result = _fixture.Create<IDocumentBase>();

        A.CallTo(() => result.Id).Returns(_fixture.Create<Guid>());
        A.CallTo(() => result.Created).Returns(DateTime.UtcNow);
        A.CallTo(() => result.Updated).Returns(result.Created);

        return result;
    }
}