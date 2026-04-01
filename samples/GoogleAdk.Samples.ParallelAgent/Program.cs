// ============================================================================
// Parallel Agent Sample
// ============================================================================
//
// Demonstrates running multiple LLM agents concurrently using ParallelAgent.
// Three perspective agents run simultaneously — each with a different viewpoint
// — and their outputs are collected as they complete.
//
// Environment variables:
//   GOOGLE_GENAI_USE_VERTEXAI=True
//   GOOGLE_CLOUD_PROJECT=<your-project-id>
//   GOOGLE_CLOUD_LOCATION=us-central1
// ============================================================================

using GoogleAdk.Core.Abstractions.Events;
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Runner;
using GoogleAdk.Models.Gemini;

var model = GeminiModelFactory.Create("gemini-2.5-flash");

// Three agents that analyze the same topic from different angles — simultaneously.
var optimist = new LlmAgent(new LlmAgentConfig
{
    Name = "optimist",
    Description = "Analyzes topics from a positive, opportunity-focused perspective.",
    Model = model,
    Instruction = """
        You are an optimistic analyst. When given a topic, identify 3 key opportunities
        and positive trends. Be specific and cite real-world examples where possible.
        Keep your response concise (under 150 words).
        """,
});

var pessimist = new LlmAgent(new LlmAgentConfig
{
    Name = "pessimist",
    Description = "Analyzes topics from a risk-focused, cautious perspective.",
    Model = model,
    Instruction = """
        You are a cautious risk analyst. When given a topic, identify 3 key risks
        and potential downsides. Be specific and practical about mitigation.
        Keep your response concise (under 150 words).
        """,
});

var pragmatist = new LlmAgent(new LlmAgentConfig
{
    Name = "pragmatist",
    Description = "Provides balanced, actionable analysis.",
    Model = model,
    Instruction = """
        You are a pragmatic strategist. When given a topic, provide 3 balanced,
        actionable recommendations that weigh both opportunities and risks.
        Focus on what someone should actually DO. Keep it under 150 words.
        """,
});

// ParallelAgent runs all three simultaneously with isolated branches
var parallelAnalysis = new ParallelAgent(new BaseAgentConfig
{
    Name = "parallel_analysis",
    Description = "Runs optimist, pessimist, and pragmatist analyses in parallel.",
    SubAgents = new List<BaseAgent> { optimist, pessimist, pragmatist },
});

var runner = new InMemoryRunner("parallel-agent-sample", parallelAnalysis);

// Create a persistent session so conversation history is preserved across turns
var session = await runner.SessionService.CreateSessionAsync(
    new GoogleAdk.Core.Abstractions.Sessions.CreateSessionRequest
    {
        AppName = "parallel-agent-sample",
        UserId = "user-1",
    });

Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
Console.WriteLine("║  ADK C# — Parallel Agent Sample                         ║");
Console.WriteLine("║  Three analysts run simultaneously on your topic.       ║");
Console.WriteLine("║  Type 'quit' to exit.                                   ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
Console.WriteLine();

while (true)
{
    Console.Write("Topic: ");
    var input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input) || input.Equals("quit", StringComparison.OrdinalIgnoreCase))
        break;

    var userMessage = new Content
    {
        Role = "user",
        Parts = new List<Part> { new() { Text = input } }
    };

    Console.WriteLine();
    var agentOutputs = new Dictionary<string, string>();

    await foreach (var evt in runner.RunAsync("user-1", session.Id, userMessage))
    {
        var text = evt.Content?.Parts?.FirstOrDefault()?.Text;
        if (text != null && evt.Partial != true && evt.Author != null)
        {
            agentOutputs[evt.Author] = text;
            Console.WriteLine($"  [{evt.Author} finished]");
        }
    }

    // Display results grouped by agent
    Console.WriteLine();
    foreach (var (agent, output) in agentOutputs.OrderBy(x => x.Key))
    {
        var emoji = agent switch
        {
            "optimist" => "🟢",
            "pessimist" => "🔴",
            "pragmatist" => "🔵",
            _ => "⚪"
        };
        Console.WriteLine($"{emoji} {agent.ToUpperInvariant()}:");
        Console.WriteLine(output);
        Console.WriteLine();
    }
    Console.WriteLine(new string('─', 60));
}
