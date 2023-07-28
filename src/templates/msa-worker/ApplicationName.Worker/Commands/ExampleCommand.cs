using ApplicationName.Worker.Contracts.Commands;

namespace ApplicationName.Worker.Commands;

public class ExampleCommand : IExampleCommand
{
    public string Name { get; set; }
}