namespace ApplicationName.Shared.Aggregates;

public interface IExampleValueObject
{
    string Code { get; }

    double Value { get; }
}