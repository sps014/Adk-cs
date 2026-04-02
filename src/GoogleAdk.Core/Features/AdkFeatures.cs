namespace GoogleAdk.Core.Features;

public enum FeatureStage
{
    Wip,
    Experimental,
    Stable
}

public enum FeatureName
{
    ToolConfirmation,
    LiveBidiStreaming,
    OutputSchemaWithTools,
    EvalFramework,
}

public sealed record FeatureConfig(FeatureStage Stage, bool DefaultOn);

public static class AdkFeatures
{
    private static readonly Dictionary<FeatureName, FeatureConfig> s_registry = new()
    {
        { FeatureName.ToolConfirmation, new FeatureConfig(FeatureStage.Stable, true) },
        { FeatureName.LiveBidiStreaming, new FeatureConfig(FeatureStage.Experimental, false) },
        { FeatureName.OutputSchemaWithTools, new FeatureConfig(FeatureStage.Experimental, false) },
        { FeatureName.EvalFramework, new FeatureConfig(FeatureStage.Experimental, false) },
    };

    private static readonly Dictionary<FeatureName, bool> s_overrides = new();

    public static bool IsFeatureEnabled(FeatureName feature)
    {
        if (s_overrides.TryGetValue(feature, out var value))
            return value;

        var enableEnv = Environment.GetEnvironmentVariable($"ADK_ENABLE_{feature}".ToUpperInvariant());
        if (IsTruthy(enableEnv)) return true;

        var disableEnv = Environment.GetEnvironmentVariable($"ADK_DISABLE_{feature}".ToUpperInvariant());
        if (IsTruthy(disableEnv)) return false;

        return s_registry.TryGetValue(feature, out var config) && config.DefaultOn;
    }

    public static IDisposable TemporaryOverride(FeatureName feature, bool enabled)
    {
        s_overrides[feature] = enabled;
        return new OverrideScope(feature);
    }

    public static void EnsureEnabled(FeatureName feature)
    {
        if (!IsFeatureEnabled(feature))
            throw new InvalidOperationException($"Feature {feature} is disabled.");
    }

    private static bool IsTruthy(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        return value.Equals("1", StringComparison.OrdinalIgnoreCase)
            || value.Equals("true", StringComparison.OrdinalIgnoreCase)
            || value.Equals("yes", StringComparison.OrdinalIgnoreCase);
    }

    private sealed class OverrideScope : IDisposable
    {
        private readonly FeatureName _feature;

        public OverrideScope(FeatureName feature) => _feature = feature;

        public void Dispose() => s_overrides.Remove(_feature);
    }
}
