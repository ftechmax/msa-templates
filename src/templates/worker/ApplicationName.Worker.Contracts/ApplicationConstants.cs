namespace ApplicationName.Worker.Contracts;

public static class ApplicationConstants
{
    public const string ApplicationKey = "ApplicationName";

    public const string DatabaseName = "ApplicationName";

    public const string ExampleCacheKey = "example";

    public const string ExampleCollectionCacheKey = $"{ExampleCacheKey}:collection";

    public static string ExampleDetailsCacheKey(Guid id) => $"{ExampleCacheKey}:details:{id:N}";

    public static string ExampleProjectionCacheKey => $"{ExampleCacheKey}:projections";

    public static string ExampleProjectionByIdCacheKey(Guid id) => $"{ExampleCacheKey}:projections:{id:N}";

    /// <summary>
    /// Returns all cache key prefixes for projection types.
    /// Add new cache key prefixes here when adding new document types.
    /// </summary>
    public static IEnumerable<string> GetCacheKeys()
    {
        yield return ExampleCacheKey;
        // Add more cache key prefixes here for other document types as needed
    }
}