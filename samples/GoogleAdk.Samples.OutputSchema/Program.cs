// ============================================================================
// Output Schema Sample — SetModelResponseTool
// ============================================================================

using GoogleAdk.Core;
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Core.Abstractions.Sessions;
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Runner;

Console.WriteLine("=== Output Schema Sample ===\n");

AdkEnv.Load();

var agent = new LlmAgent(new LlmAgentConfig
{
    Name = "schema",
    Model = "gemini-2.5-flash",
    Instruction = "Return a JSON response that matches the schema using set_model_response.",
    OutputSchema = typeof(SchemaOutput)
});

// ── Console mode ───────────────────────────────────────────────────────────

await ConsoleRunner.RunAsync(agent);

Console.WriteLine("\nDone!");

public class SchemaOutput
{
    public string? Foo { get; set; }
}