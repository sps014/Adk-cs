using GoogleAdk.Core;
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Tools;
using ModelContextProtocol.Client;

namespace GoogleAdk.Tools.Mcp;

/// <summary>
/// A toolset that dynamically discovers and provides tools from an MCP server.
/// Connects to the server, retrieves available tools, and wraps each in an <see cref="McpTool"/>.
/// </summary>
/// <example>
/// <code>
/// // Stdio-based MCP server
/// var toolset = new McpToolset(new StdioConnectionParams
/// {
///     Command = "npx",
///     Arguments = ["-y", "@modelcontextprotocol/server-everything"]
/// });
///
/// // HTTP-based MCP server
/// var toolset = new McpToolset(new HttpConnectionParams
/// {
///     Url = "http://localhost:8788/mcp"
/// });
///
/// // Use with an LlmAgent
/// var agent = new LlmAgent(new LlmAgentConfig
/// {
///     Name = "mcp_agent",
///     Model = model,
///     Instruction = "You are a helpful assistant.",
///     Toolsets = new List&lt;BaseToolset&gt; { toolset }
/// });
/// </code>
/// </example>
public sealed class McpToolset : BaseToolset
{
    private readonly McpSessionManager _sessionManager;

    public McpToolset(McpConnectionParams connectionParams, string? prefix = null)
        : base(prefix: prefix)
    {
        _sessionManager = new McpSessionManager(connectionParams);
    }

    public McpToolset(McpConnectionParams connectionParams, IReadOnlyList<string> toolFilterNames, string? prefix = null)
        : base(toolFilterNames, prefix)
    {
        _sessionManager = new McpSessionManager(connectionParams);
    }

    public McpToolset(McpConnectionParams connectionParams, ToolPredicate toolFilter, string? prefix = null)
        : base(toolFilter, prefix)
    {
        _sessionManager = new McpSessionManager(connectionParams);
    }

    /// <summary>
    /// Connects to the MCP server and returns all available tools (filtered by any configured filter).
    /// </summary>
    public override async Task<IReadOnlyList<BaseTool>> GetToolsAsync(AgentContext? context = null)
    {
        await using var client = await _sessionManager.CreateSessionAsync();
        var mcpTools = await client.ListToolsAsync();

        var tools = new List<BaseTool>();
        foreach (var mcpTool in mcpTools)
        {
            var wrapped = new McpTool(mcpTool, _sessionManager, Prefix);

            // Apply filters
            if (ToolFilterNames != null && !ToolFilterNames.Contains(mcpTool.Name))
                continue;
            if (ToolFilterPredicate != null && context != null && !ToolFilterPredicate(wrapped, context))
                continue;

            tools.Add(wrapped);
        }

        return tools;
    }

    public override async ValueTask DisposeAsync()
    {
        await Task.CompletedTask;
    }
}
