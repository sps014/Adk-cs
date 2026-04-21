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

var agent = new LlmAgent(new LlmAgentConfig
{
    Name = "planner",
    Model = "gemini-2.5-flash",
    Planner = new PlanReActPlanner(),
    Instruction = "You are a helpful assistant."
});

// ── Console mode ───────────────────────────────────────────────────────────

await ConsoleRunner.RunAsync(agent);

Console.WriteLine("\n=== Planning Sample Complete ===");

