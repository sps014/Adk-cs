// ============================================================================
// Feature Flags + App Container Sample
// ============================================================================

using GoogleAdk.Core;
using GoogleAdk.Core.Abstractions.Events;
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Core.Abstractions.Sessions;
using GoogleAdk.Core.Abstractions.Tools;
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Apps;
using GoogleAdk.Core.Features;
using GoogleAdk.Core.Plugins;
using GoogleAdk.Core.Context;
using GoogleAdk.Models.Gemini;
using GoogleAdk.Samples.FeatureFlags;
using GoogleAdk.Core;

AdkEnv.Load();

Console.WriteLine("=== Feature Flags + App Container Sample ===\n");

using var _ = AdkFeatures.TemporaryOverride(FeatureName.LiveBidiStreaming, true);
Console.WriteLine($"LiveBidi enabled: {AdkFeatures.IsFeatureEnabled(FeatureName.LiveBidiStreaming)}");

AdkEnv.Load();

var model = CreateGeminiModel("gemini-2.5-flash");
var agent = new LlmAgent(new LlmAgentConfig
{
    Name = "app-agent",
    Model = model,
    Instruction = "Call render_widget at least once and then respond briefly.",
    ContextCacheConfig = new ContextCacheConfig { CacheKey = "sample" },
    Tools =
    [
        SampleFeatureTools.RenderWidgetTool
    ]
});

var app = new AdkApp("feature-sample", agent)
{
    Plugins = [new GlobalInstructionPlugin("Always be concise.")]
};

var runner = new GoogleAdk.Core.Runner.Runner(new GoogleAdk.Core.Runner.RunnerConfig
{
    AppName = "feature-sample",
    App = app,
    SessionService = new GoogleAdk.Core.Sessions.InMemorySessionService()
});

var session = await runner.SessionService.CreateSessionAsync(new CreateSessionRequest
{
    AppName = "feature-sample",
    UserId = "user-1"
});

var userMessage = new Content { Role = "user", Parts = [new Part { Text = "Hi" }] };
try
{
    await foreach (var evt in runner.RunAsync("user-1", session.Id, userMessage))
    {
        if (evt.Actions.RenderUiWidgets.Count > 0)
            Console.WriteLine($"UI Widgets: {evt.Actions.RenderUiWidgets.Count}");
    }
}
catch (Exception ex)
{
    PrintGeminiAuthHelp(ex);
    return;
}

Console.WriteLine("\n=== Feature Flags Sample Complete ===");

static BaseLlm CreateGeminiModel(string modelName)
{
    try
    {
        return GeminiModelFactory.Create(modelName);
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException(
            "Failed to create Gemini model. Ensure GOOGLE_API_KEY is set for AI Studio, " +
            "or set GOOGLE_GENAI_USE_VERTEXAI=True with GOOGLE_CLOUD_PROJECT/LOCATION and ADC.",
            ex);
    }
}

static void PrintGeminiAuthHelp(Exception ex)
{
    Console.Error.WriteLine("Gemini request failed. Verify credentials:");
    Console.Error.WriteLine("- AI Studio: set GOOGLE_API_KEY and GOOGLE_GENAI_USE_VERTEXAI=0");
    Console.Error.WriteLine("- Vertex AI: set GOOGLE_GENAI_USE_VERTEXAI=True and run `gcloud auth application-default login`");
    Console.Error.WriteLine();
    Console.Error.WriteLine(ex);
}
