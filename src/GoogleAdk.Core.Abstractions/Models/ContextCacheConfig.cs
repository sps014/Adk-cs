namespace GoogleAdk.Core.Abstractions.Models;

/// <summary>
/// Configures context caching for a model, controlling how often a cache is
/// created, how long it lives, and the minimum payload size worth caching.
/// </summary>
public sealed class ContextCacheConfig
{
    /// <summary>How many invocations a single cache entry is reused before being refreshed.</summary>
    public int CacheIntervals { get; set; } = 10;

    /// <summary>The time-to-live for a cache entry, in seconds.</summary>
    public int TtlSeconds { get; set; } = 1800;

    /// <summary>The minimum token count required before content is cached.</summary>
    public int MinTokens { get; set; } = 0;

    /// <summary>The TTL formatted as the duration string the backend expects (e.g. <c>"1800s"</c>).</summary>
    public string TtlString => $"{TtlSeconds}s";

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"ContextCacheConfig(CacheIntervals={CacheIntervals}, Ttl={TtlString}, MinTokens={MinTokens})";
    }
}
