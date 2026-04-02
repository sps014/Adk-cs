// ============================================================================
// Planning Sample — PlanReActPlanner
// ============================================================================

using GoogleAdk.Core;
using GoogleAdk.Core.Abstractions.Events;
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Planning;
using GoogleAdk.Core.Runner;
using GoogleAdk.Models.Gemini;

Console.WriteLine("=== Planning Sample ===\n");

AdkEnv.Load();

var model = CreateGeminiModel("gemini-2.5-flash");
var agent = new LlmAgent(new LlmAgentConfig
{
    Name = "planner",
    Model = model,
    Planner = new PlanReActPlanner(),
    Instruction = "You are a helpful assistant."
});

var runner = new InMemoryRunner("planning-sample", agent);
var session = await runner.SessionService.CreateSessionAsync(new GoogleAdk.Core.Abstractions.Sessions.CreateSessionRequest
{
    AppName = "planning-sample",
    UserId = "user-1"
});

var userMessage = new Content
{
    Role = "user",
    Parts = [new Part { Text = "Plan a weekend in Paris." }]
};

try
{
    await foreach (var evt in runner.RunAsync("user-1", session.Id, userMessage))
    {
        var text = evt.Content?.Parts?.FirstOrDefault()?.Text;
        if (!string.IsNullOrWhiteSpace(text))
            Console.WriteLine(text);
    }
}
catch (Exception ex)
{
    PrintGeminiAuthHelp(ex);
    return;
}

Console.WriteLine("\n=== Planning Sample Complete ===");

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
