using GoogleAdk.Core.Agents;

namespace GoogleAdk.Core.Tools;

/// <summary>
/// Loads MCP resources into the session (placeholder).
/// </summary>
public sealed class LoadMcpResourceTool : BaseTool
{
    public LoadMcpResourceTool()
        : base("load_mcp_resource", "Loads a resource from an MCP server.")
    {
    }

    public override Task<object?> RunAsync(Dictionary<string, object?> args, AgentContext context)
    {
        return Task.FromResult<object?>(null);
    }
}
