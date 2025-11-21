namespace ApplicationName.Api.Contracts;

public static class ApplicationConstants
{
    public const string ApplicationKey = "applicationname";

    public const string ExampleCacheKey = "example";

    public const string ExampleCollectionCacheKey = $"{ExampleCacheKey}:collection";

    public static string ExampleDetailsCacheKey(Guid id) => $"{ExampleCacheKey}:details:{id:N}";

    public static string ExampleProjectionCacheKey => $"projections:{ExampleCacheKey}:*";

    public static string ExampleProjectionByIdCacheKey(Guid id) => $"projections:{ExampleCacheKey}:{id:N}";
}