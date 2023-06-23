using ApplicationName.Api.Contracts.Test.Commands;
using FakeItEasy;
using System.Collections.Generic;

namespace ApplicationName.Api.Application.Test;

// ReSharper disable once UnusedMember.Global
public class TestBootstrap : DefaultBootstrapper
{
    public override IEnumerable<string> GetAssemblyFileNamesToScanForExtensions()
    {
        return new[]
        {
            typeof(ExampleCommandDummyFactory).Assembly.Location
        };
    }
}