using System.Collections.Generic;
using ApplicationName.Shared.Test.Aggregates;
using FakeItEasy;

namespace ApplicationName.Api.Test;

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