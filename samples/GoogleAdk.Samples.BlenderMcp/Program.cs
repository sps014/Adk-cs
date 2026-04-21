// ============================================================================
// Blender MCP Sample — Stdio + uvx
// MCP Repo
// https://github.com/ahujasid/blender-mcp
// ============================================================================

//
// Prereqs:
//   - Blender installed
//  -  blender-mcp addon installed in blender  and mcp server running in blender 
//   - Install uvx (uv) 
//   - Ensure "uvx" is on PATH
//
// Usage:
//   dotnet run --project samples/GoogleAdk.Samples.BlenderMcp
// ============================================================================

using GoogleAdk.Core;
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Core.Abstractions.Sessions;
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Runner;
using GoogleAdk.Models.Gemini;
using GoogleAdk.Tools.Mcp;

AdkEnv.Load();

var model = GeminiModelFactory.Create("gemini-2.5-flash");

var blenderToolset = new McpToolset(new StdioConnectionParams
{
    Command = "uvx",
    Arguments = ["-q", "blender-mcp"],
    TimeoutMs = 60_000
});

var agent = new LlmAgent(new LlmAgentConfig
{
    Name = "blender_mcp_agent",
    Description = "An agent that uses Blender MCP tools over stdio.",
    Model = model,
    Instruction = """
        You can use Blender MCP tools to perform modeling, scene, or render tasks.
        Be explicit about which tool you call and what it returns.
        """,
    Tools = [blenderToolset]
});

await ConsoleRunner.RunAsync(agent);

await blenderToolset.DisposeAsync();
Console.WriteLine("Goodbye!");
