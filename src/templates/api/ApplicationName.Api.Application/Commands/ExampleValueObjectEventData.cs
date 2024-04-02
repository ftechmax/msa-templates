using ApplicationName.Shared.Events;

namespace ApplicationName.Api.Application.Commands;

public class ExampleValueObjectEventData : IExampleValueObjectEventData
{
    public string Code { get; set; }

    public double Value { get; set; }
}