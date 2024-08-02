namespace ApplicationName.Api.Contracts;

public static class ApplicationConstants
{
    public static readonly Uri MessageEndpoint = new("queue:ApplicationName.Worker");

    public const string ExampleCollectionCacheKey = "example-collection";

    public const string ApplicationKey = "applicationname";

    public const string ExampleCacheKey = "example";

    public const string ExampleCollectionCacheKey = $"{ExampleCacheKey}:collection";

    public static string ExampleDetailsCacheKey(Guid id) => $"{ExampleCacheKey}:details:{id:N}";
}