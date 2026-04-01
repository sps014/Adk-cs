using System.Text.RegularExpressions;

namespace GoogleAdk.Core.Abstractions.Models;

/// <summary>
/// Registry for mapping model name patterns to LLM factories.
/// </summary>
public static class LlmRegistry
{
    private sealed record Entry(Regex Pattern, Func<string, BaseLlm> Factory);

    private static readonly List<Entry> Entries = new();
    private static readonly Dictionary<string, Func<string, BaseLlm>> Cache = new();
    private const int CacheLimit = 32;

    public static void Register(Func<string, BaseLlm> factory, IEnumerable<string> modelPatterns)
    {
        foreach (var pattern in modelPatterns)
        {
            if (string.IsNullOrWhiteSpace(pattern)) continue;
            var regex = new Regex($"^{pattern}$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            Entries.Add(new Entry(regex, factory));
        }
        Cache.Clear();
    }

    public static BaseLlm NewLlm(string model)
    {
        var factory = Resolve(model);
        return factory(model);
    }

    public static Func<string, BaseLlm> Resolve(string model)
    {
        if (Cache.TryGetValue(model, out var cached))
            return cached;

        foreach (var entry in Entries)
        {
            if (entry.Pattern.IsMatch(model))
            {
                AddToCache(model, entry.Factory);
                return entry.Factory;
            }
        }
        throw new InvalidOperationException($"No LLM registered for model \"{model}\".");
    }

    public static void Clear()
    {
        Entries.Clear();
        Cache.Clear();
    }

    private static void AddToCache(string model, Func<string, BaseLlm> factory)
    {
        if (Cache.Count >= CacheLimit)
        {
            var keyToRemove = Cache.Keys.First();
            Cache.Remove(keyToRemove);
        }
        Cache[model] = factory;
    }
}
