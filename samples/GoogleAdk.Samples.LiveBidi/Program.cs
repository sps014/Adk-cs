// ============================================================================
// Live/Bidi Sample — LiveRequestQueue + RunLiveAsync
// ============================================================================

using GoogleAdk.Core;
using GoogleAdk.Core.Abstractions.Events;
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Core.Abstractions.Sessions;
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Runner;
using GoogleAdk.Models.Gemini;

Console.WriteLine("=== Live/Bidi Sample ===\n");

AdkEnv.Load();

var model = CreateGeminiModel("gemini-2.5-flash");
var agent = new LlmAgent(new LlmAgentConfig { Name = "live", Model = model });
var runner = new InMemoryRunner("live-sample", agent);
var session = await runner.SessionService.CreateSessionAsync(new CreateSessionRequest
{
    AppName = "live-sample",
    UserId = "user-1"
});

var queue = new LiveRequestQueue();
var events = new List<Event>();
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));

var runTask = Task.Run(async () =>
{
    await foreach (var evt in runner.RunLiveAsync("user-1", session.Id, queue, cancellationToken: cts.Token))
    {
        events.Add(evt);
        var text = evt.Content?.Parts?.FirstOrDefault()?.Text;
        if (!string.IsNullOrWhiteSpace(text))
        {
            cts.Cancel();
            break;
        }
    }
});

await queue.SendContentAsync(new Content { Role = "user", Parts = [new Part { Text = "ping" }] });
queue.Close();
try
{
    await runTask;
}
catch (Exception ex)
{
    PrintGeminiAuthHelp(ex);
    return;
}

foreach (var evt in events)
{
    var text = evt.Content?.Parts?.FirstOrDefault()?.Text;
    if (!string.IsNullOrWhiteSpace(text))
        Console.WriteLine($"Model: {text}");
}

Console.WriteLine("\n=== Live/Bidi Sample Complete ===");

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
