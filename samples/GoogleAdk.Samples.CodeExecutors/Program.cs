// ============================================================================
// Code Executors Sample — LLM + Built-in Code Execution
// ============================================================================
//
// Demonstrates:
//   1. LlmAgent configured with BuiltInCodeExecutor
//   2. Executable code + execution results returned in events
// ============================================================================

using GoogleAdk.Core;
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Core.Abstractions.Sessions;
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.CodeExecutors;
using GoogleAdk.Core.Runner;

AdkEnv.Load();

Console.WriteLine("=== Code Executors Sample (LLM) ===\n");

var agent = new LlmAgent(new LlmAgentConfig
{
    Name = "code",
    Model = "gemini-2.5-flash",
    Instruction = "Use Python code execution for calculations.",
    CodeExecutor = new BuiltInCodeExecutor()
});

// ── Console mode ───────────────────────────────────────────────────────────

await ConsoleRunner.RunAsync(agent);

Console.WriteLine("\n=== Code Executors Sample Complete ===");
