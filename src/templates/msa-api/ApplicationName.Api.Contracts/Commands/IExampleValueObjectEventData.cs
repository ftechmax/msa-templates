// ReSharper disable once CheckNamespace
namespace ApplicationName.Worker.Contracts.Events;

public interface IExampleValueObjectEventData
{
    string Code { get; }

    double Value { get; }
}