namespace GoogleAdk.Core.Tools;

/// <summary>
/// Shared helpers for the Google Cloud tools (BigQuery, Bigtable, Spanner,
/// Pub/Sub, API Hub, Discovery Engine, etc.).
/// </summary>
/// <remarks>
/// These tools historically repeated the same argument parsing and result
/// shaping at every call site. Centralizing them here keeps the result
/// contract (the <c>status</c>/<c>error_details</c>/<c>error</c> keys that
/// callers and tests rely on) consistent across every cloud tool.
/// </remarks>
internal static class CloudTool
{
    /// <summary>Reads a string argument, returning <see langword="null"/> when missing or empty.</summary>
    public static string? GetString(Dictionary<string, object?> args, string name)
        => args.TryGetValue(name, out var value) ? FunctionToolArgs.Get<string>(value) : null;

    /// <summary>Reads an optional typed argument, returning <paramref name="defaultValue"/> when missing.</summary>
    public static T? GetOrDefault<T>(Dictionary<string, object?> args, string name, T? defaultValue = default)
        => args.TryGetValue(name, out var value) && value is not null ? FunctionToolArgs.Get<T>(value) : defaultValue;

    /// <summary>Builds the standard "missing required argument" error result.</summary>
    public static Dictionary<string, object?> MissingArgument(string name)
        => new() { ["error"] = $"{name} is required." };

    /// <summary>Builds the standard error result from a caught exception.</summary>
    public static Dictionary<string, object?> Error(Exception exception)
        => new() { ["status"] = "ERROR", ["error_details"] = exception.Message };

    /// <summary>Builds a successful result, always including <c>status = "SUCCESS"</c>.</summary>
    public static Dictionary<string, object?> Success(params (string Key, object? Value)[] fields)
    {
        var result = new Dictionary<string, object?> { ["status"] = "SUCCESS" };
        foreach (var (key, value) in fields)
            result[key] = value;
        return result;
    }
}
