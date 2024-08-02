using ApplicationName.Shared;

namespace ApplicationName.Api.Contracts;

public static class ApplicationConstants
{
    public const string MessageEndpoint = "queue:ApplicationName.Worker";

    public const string ApplicationKey = "applicationname";

    public const string ExampleCollectionCacheKey = $"{ProjectionConstants.ExampleKey}:collection";

    public const string ExampleDetailsCacheNamespace = $"{ProjectionConstants.ExampleKey}:details";

    public static string ExampleDetailsCacheKey(Guid id) => $"{ExampleDetailsCacheNamespace}:{id:N}";
}