using ApplicationName.Shared.Events;

namespace ApplicationName.Worker.Events;

public class ExampleValueObjectEventData : IExampleValueObjectEventData
{
    public string Code { get; set; }

    public double Value { get; set; }
}