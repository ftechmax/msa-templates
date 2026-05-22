using ApplicationName.Shared.Test.Aggregates;
using FakeItEasy;

namespace ApplicationName.Api.Application.Test;

// ReSharper disable once UnusedMember.Global
public class TestBootstrap : DefaultBootstrapper
{
    public override IEnumerable<string> GetAssemblyFileNamesToScanForExtensions()
    {
        return new[]
        {
            typeof(AggregateDummyFactory).Assembly.Location
        };
    }
}