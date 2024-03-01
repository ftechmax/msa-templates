﻿using ApplicationName.Shared.Aggregates;
using ApplicationName.Shared.Events;
using ArgDefender;
using MongoDB.Bson.Serialization.Attributes;

namespace ApplicationName.Worker.Application.Documents;

public sealed class ExampleValueObject : IExampleValueObject
{
    [BsonConstructor]
    private ExampleValueObject()
    {
    }

    public ExampleValueObject(IExampleValueObjectEventData eventData)
        : this()
    {
        Guard.Argument(eventData).NotNull();
        Guard.Argument(eventData.Code).NotNull().NotWhiteSpace();
        Guard.Argument(eventData.Value).NotNegative();

        Code = eventData.Code;
        Value = eventData.Value;
    }

    [BsonElement]
    public string Code { get; }

    [BsonElement]
    public double Value { get; }
}