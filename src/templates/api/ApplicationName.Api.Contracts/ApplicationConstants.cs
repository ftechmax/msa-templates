namespace ApplicationName.Api.Contracts;

public static class ApplicationConstants
{
    public static readonly Uri MessageEndpoint = new("queue:ApplicationName.Worker");

    public const string ApplicationKey = "applicationname";

    public const string ExampleCacheKey = "example";

    public const string ExampleProjectionCacheNamespace = $"{ExampleCacheKey}:projection";

    public static string ExampleProjectionCacheKey(Guid id) => $"{ExampleProjectionCacheNamespace}:{id:N}";

    public const string ExampleCollectionCacheKey = $"{ExampleCacheKey}:collection";

    public const string ExampleDetailsCacheNamespace = $"{ExampleCacheKey}:details";

    public static string ExampleDetailsCacheKey(Guid id) => $"{ExampleDetailsCacheNamespace}:{id:N}";
}