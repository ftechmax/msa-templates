using ApplicationName.Shared.Test.Aggregates;
using FakeItEasy;

namespace ApplicationName.Worker.Application.Test;

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