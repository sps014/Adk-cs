namespace GoogleAdk.Core.Features;

/// <summary>
/// Marks a class or member as experimental and checks the feature flag on first use.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property)]
public sealed class ExperimentalAttribute : Attribute
{
    public FeatureName Feature { get; }

    public ExperimentalAttribute(FeatureName feature)
    {
        Feature = feature;
    }
}
