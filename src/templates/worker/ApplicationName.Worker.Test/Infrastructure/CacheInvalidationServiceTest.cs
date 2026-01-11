using ApplicationName.Worker.Infrastructure;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using NUnit.Framework;
using Shouldly;

namespace ApplicationName.Worker.Test.Infrastructure;

public class CacheInvalidationServiceTest
{
    private IFixture _fixture;

    private CacheInvalidationService _subjectUnderTest;


    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization());
        _subjectUnderTest = _fixture.Create<CacheInvalidationService>();
    }

    [Test]
    public void TryParseProjectionKey_WhenValid_ReturnsPrefixAndId()
    {
        var id = Guid.NewGuid();
        var key = $"projections:example:{id:N}";

        var result = CacheInvalidationService.TryParseProjectionKey(key, out var prefix, out var parsedId);

        result.ShouldBeTrue();
        prefix.ShouldBe("example");
        parsedId.ShouldBe(id);
    }

    [Test]
    public void TryParseProjectionKey_WhenInvalid_ReturnsFalse()
    {
        var key = "projections:example:not-a-guid";

        var result = CacheInvalidationService.TryParseProjectionKey(key, out var prefix, out var parsedId);

        result.ShouldBeFalse();
        prefix.ShouldBe(string.Empty);
        parsedId.ShouldBe(Guid.Empty);
    }

    [Test]
    public async Task TryEnqueueKey_WhenQueueCompleted_DoesNotHoldKey()
    {
        var key = $"projections:example:{Guid.NewGuid():N}";

        await _subjectUnderTest.StopAsync(CancellationToken.None);

        _subjectUnderTest.TryEnqueueKey(key).ShouldBeFalse();
        _subjectUnderTest.IsQueuedOrRunning(key).ShouldBeFalse();
    }

    [Test]
    public void TryEnqueueKey_WhenDuplicate_DoesNotEnqueueAgain()
    {
        var key = $"projections:example:{Guid.NewGuid():N}";

        _subjectUnderTest.TryEnqueueKey(key).ShouldBeTrue();
        _subjectUnderTest.TryEnqueueKey(key).ShouldBeFalse();
    }
}
