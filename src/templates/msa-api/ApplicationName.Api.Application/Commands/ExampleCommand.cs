using ApplicationName.Api.Contracts.Commands;

namespace ApplicationName.Api.Application.Commands;

public class ExampleCommand : IExampleCommand
{
    public string Name { get; set; }
}