namespace ApplicationName.Api.Contracts;

public static class ApplicationConstants
{
    public static readonly Uri MessageEndpoint = new("queue:ApplicationName.Worker");

    public const string DatabaseName = "ApplicationName";

    public const string ExampleCollectionCacheKey = "example-collection";

    public static string ExampleDetailsCacheKey(Guid id) => $"example-details_{id:N}";
}