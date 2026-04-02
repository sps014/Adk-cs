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

var runner = new InMemoryRunner("blender-mcp-sample", agent);
var session = await runner.SessionService.CreateSessionAsync(new CreateSessionRequest
{
    AppName = "blender-mcp-sample",
    UserId = "user-1"
});

Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
Console.WriteLine("║  ADK C# — Blender MCP Sample                             ║");
Console.WriteLine("║  Type a prompt or 'quit' to exit.                        ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
Console.WriteLine();

while (true)
{
    Console.Write("You: ");
    var input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input) || input.Equals("quit", StringComparison.OrdinalIgnoreCase))
        break;

    var userMessage = new Content
    {
        Role = "user",
        Parts = [new Part { Text = input }]
    };

    await foreach (var evt in runner.RunAsync("user-1", session.Id, userMessage))
    {
        foreach (var call in evt.GetFunctionCalls())
            Console.WriteLine($"  ⚡ Tool call: {call.Name}");

        if (evt.IsFinalResponse() && evt.Content?.Parts != null)
        {
            foreach (var part in evt.Content.Parts)
            {
                if (!string.IsNullOrWhiteSpace(part.Text))
                    Console.WriteLine($"Agent: {part.Text}");
            }
        }
    }
}

await blenderToolset.DisposeAsync();
Console.WriteLine("Goodbye!");
