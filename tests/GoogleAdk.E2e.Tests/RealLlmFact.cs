using GoogleAdk.Core;

namespace GoogleAdk.E2e.Tests;

/// <summary>
/// Shared configuration helpers for tests that hit a real Gemini/Vertex backend.
/// </summary>
public static class RealLlmTestConfig
{
    /// <summary>
    /// True when credentials for a real model call are available, either a Gemini API
    /// key (<c>GOOGLE_API_KEY</c>/<c>GEMINI_API_KEY</c>) or a Vertex AI configuration
    /// (<c>GOOGLE_GENAI_USE_VERTEXAI=true</c> with a project set).
    /// </summary>
    public static bool IsConfigured { get; } = ResolveIsConfigured();

    /// <summary>Reason string used to skip real-LLM tests when no credentials are present.</summary>
    public const string SkipReason =
        "No real-LLM credentials configured. Set GOOGLE_API_KEY (or Vertex AI env) in tests/GoogleAdk.E2e.Tests/.env to run.";

    private static bool ResolveIsConfigured()
    {
        // AdkEnv.Load() is already invoked by the module initializer, but call it again
        // defensively so the check is correct regardless of initialization ordering.
        try { AdkEnv.Load(); } catch { /* best-effort */ }

        var apiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY")
                     ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        if (!string.IsNullOrWhiteSpace(apiKey) && apiKey != "your_api_key")
            return true;

        var useVertex = Environment.GetEnvironmentVariable("GOOGLE_GENAI_USE_VERTEXAI");
        var project = Environment.GetEnvironmentVariable("GOOGLE_CLOUD_PROJECT");
        if (string.Equals(useVertex, "true", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(project))
            return true;

        return false;
    }
}

/// <summary>
/// A <see cref="FactAttribute"/> that automatically skips when no real-LLM credentials
/// are configured, so integration tests are runnable locally (with a key) yet never
/// fail a credential-less CI run.
/// </summary>
public sealed class RealLlmFactAttribute : FactAttribute
{
    public RealLlmFactAttribute()
    {
        if (!RealLlmTestConfig.IsConfigured)
            Skip = RealLlmTestConfig.SkipReason;
    }
}
