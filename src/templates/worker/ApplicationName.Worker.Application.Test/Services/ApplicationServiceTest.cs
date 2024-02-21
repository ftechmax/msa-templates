using ApplicationName.Worker.Application.Services;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using NUnit.Framework;

namespace ApplicationName.Worker.Application.Test.Services;

public class ApplicationServiceTest
{
    private IFixture _fixture;

    private IDocumentRepository _documentRepository;

    private ApplicationService _subjectUnderTest;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization());

        _documentRepository = _fixture.Freeze<IDocumentRepository>();

        _subjectUnderTest = _fixture.Create<ApplicationService>();
    }
}