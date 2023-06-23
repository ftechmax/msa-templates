using ApplicationName.Worker.Contracts.Test.Commands;
using FakeItEasy;
using System.Collections.Generic;

namespace ApplicationName.Worker.Application.Test;

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