using System.Collections.Generic;
using ApplicationName.Shared.Test.Commands;
using FakeItEasy;

namespace ApplicationName.Worker.Application.Test;

public class TestBootstrap : DefaultBootstrapper
{
    public override IEnumerable<string> GetAssemblyFileNamesToScanForExtensions()
    {
        return new[]
        {
            typeof(CreateExampleCommandDummyFactory).Assembly.Location
        };
    }
}