using ApplicationName.Shared.Projections;
using ApplicationName.Worker.Application;
using ApplicationName.Worker.Application.Documents;
using ApplicationName.Worker.Contracts;

namespace ApplicationName.Worker.Infrastructure;

public record CacheProjectionMapping(Type DocumentType, Type ProjectionType, string CacheKeyPrefix);

/// <summary>
/// Static registry for cache projection mappings.
/// Add new document-projection mappings here.
/// </summary>
public static class CacheProjectionRegistry
{
    private static readonly List<CacheProjectionMapping> Mappings =
    [
        new CacheProjectionMapping(typeof(ExampleDocument), typeof(ExampleProjection), ApplicationConstants.ExampleCacheKey)

        // Add more mappings here for other document types        
    ];

    public static IEnumerable<CacheProjectionMapping> GetMappings()
    {
        return Mappings;
    }
}

