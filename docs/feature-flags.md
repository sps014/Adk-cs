# Feature Flags

The ADK uses feature flags to safely roll out experimental components, manage breaking changes, and offer developers the ability to opt-in or opt-out of specific internal behaviors dynamically.

## Enabling Features

Feature flags are evaluated dynamically via the `AdkFeatures` utility class. The most robust way to enable a feature is by setting the associated environment variable prior to application startup.

Flags generally follow the naming convention `ADK_ENABLE_FEATURENAME`.

```csharp
using GoogleAdk.Core.Features;

// Enable via Environment Variables before AdkEnv is loaded
Environment.SetEnvironmentVariable("ADK_ENABLE_LIVEBIDISTREAMING", "1");
Environment.SetEnvironmentVariable("ADK_ENABLE_OUTPUTSCHEMA_WITH_TOOLS", "True");

AdkEnv.Load();
```

## Temporary Overrides

If you need to test a feature flag temporarily without polluting the global environment (for instance, during unit tests), you can utilize the `TemporaryOverride` block.

```csharp
using GoogleAdk.Core.Features;

using (AdkFeatures.TemporaryOverride(FeatureName.OutputSchemaWithTools, true))
{
    // The feature is dynamically active here
    Assert.True(AdkFeatures.IsFeatureEnabled(FeatureName.OutputSchemaWithTools));
    
    // Execute logic heavily dependent on Output Schema configurations
}

// Upon exiting the using block, the feature flag automatically reverts.
Assert.False(AdkFeatures.IsFeatureEnabled(FeatureName.OutputSchemaWithTools));
```