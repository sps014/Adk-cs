// ============================================================================
// Google Search Agent Sample
// ============================================================================
//
// Demonstrates Gemini's built-in Google Search grounding, which lets the model
// fetch real-time information from the web without custom tool code.
//
// Environment variables:
//   GOOGLE_GENAI_USE_VERTEXAI=True
//   GOOGLE_CLOUD_PROJECT=<your-project-id>
//   GOOGLE_CLOUD_LOCATION=us-central1
// ============================================================================

using GoogleAdk.Core;
using GoogleAdk.Core.Abstractions.Events;
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Runner;
using GoogleAdk.Core.Tools;
using GoogleAdk.Models.Gemini;

AdkEnv.Load();

var model = GeminiModelFactory.Create("gemini-2.5-flash");

var searchAgent = new LlmAgent(new LlmAgentConfig
{
    Name = "search_agent",
    Description = "An agent with real-time web search capability via Gemini grounding.",
    Model = model,
    Instruction = """
        You are a helpful research assistant with real-time web access via Google Search.
        When asked about current events, recent data, or factual questions, use your
        search capability to find accurate, up-to-date information.
        Always cite your sources and provide context for your answers.
        """,
    Tools = new List<IBaseTool> { new GoogleSearchTool() },
});

// ── Console mode ───────────────────────────────────────────────────────────

await ConsoleRunner.RunAsync(searchAgent);
