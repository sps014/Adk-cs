using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GoogleAdk.Core.Abstractions.Json;

/// <summary>
/// Shared, reusable <see cref="JsonSerializerOptions"/> profiles for the ADK.
/// </summary>
/// <remarks>
/// <see cref="JsonSerializerOptions"/> instances are expensive to build and are
/// frozen on first use, so the recommended pattern is to share a small set of
/// cached, read-only instances rather than allocating a new options object at
/// every call site. These profiles replace the previously duplicated inline
/// option objects scattered across the codebase.
/// </remarks>
public static class AdkJson
{
    /// <summary>camelCase property names, omitting <see langword="null"/> values when writing.</summary>
    public static JsonSerializerOptions CamelCaseIgnoreNull { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>camelCase property names with case-insensitive deserialization.</summary>
    public static JsonSerializerOptions CamelCaseCaseInsensitive { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>Case-insensitive deserialization with default naming.</summary>
    public static JsonSerializerOptions CaseInsensitive { get; } = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>Compact (single-line) output that omits <see langword="null"/> values; useful for debug logs.</summary>
    public static JsonSerializerOptions CompactIgnoreNull { get; } = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>Indented output that omits <see langword="null"/> values.</summary>
    public static JsonSerializerOptions IndentedIgnoreNull { get; } = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>Indented output with relaxed escaping; useful for human-readable console rendering.</summary>
    public static JsonSerializerOptions IndentedRelaxed { get; } = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };
}
