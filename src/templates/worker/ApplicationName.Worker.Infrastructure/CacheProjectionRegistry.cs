using ApplicationName.Shared.Projections;
using ApplicationName.Worker.Application.Documents;
using ApplicationName.Worker.Contracts;

namespace ApplicationName.Worker.Infrastructure;

public record CacheProjectionMapping(Type DocumentType, Type ProjectionType, string CacheKeyPrefix);

/// <summary>
/// Static registry for cache projection mappings.
/// </summary>
public static class CacheProjectionRegistry
{
    public static readonly IReadOnlyCollection<CacheProjectionMapping> Mappings = Array.AsReadOnly(
    [
        new CacheProjectionMapping(typeof(ExampleDocument), typeof(ExampleProjection), ApplicationConstants.ExampleCacheKey)

        // Add more mappings here for other document types
    ]);
}
