namespace ApplicationName.Shared.Events;

public interface IExampleValueObjectEventData
{
    string Code { get; }

    double Value { get; }
}