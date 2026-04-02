// ============================================================================
// Output Schema Sample — SetModelResponseTool
// ============================================================================

using GoogleAdk.Core;
using GoogleAdk.Core.Abstractions.Events;
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Core.Abstractions.Sessions;
using GoogleAdk.Core.Abstractions.Tools;
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Runner;
using GoogleAdk.Models.Gemini;
using GoogleAdk.Samples.OutputSchema;
using GoogleAdk.Dev;

Console.WriteLine("=== Output Schema Sample ===\n");

AdkEnv.Load();
var agent = new LlmAgent(new LlmAgentConfig
{
    Name = "schema",
    ModelName = "gemini-2.5-flash",
    Instruction = "Return a JSON response that matches the schema using set_model_response.",
    Tools =
    [
        SampleSchemaTools.NoopTool
    ],
    OutputSchema = new Dictionary<string, object?>
    {
        ["type"] = "object",
        ["properties"] = new Dictionary<string, object?>
        {
            ["foo"] = new Dictionary<string, object?> { ["type"] = "string" }
        }
    }
});

await AdkWeb.RunAsync(agent);