# Model Context Protocol (MCP)

The ADK supports the Model Context Protocol (MCP), allowing your agents to dynamically discover and use tools provided by external MCP servers.

## Core Components

- **`McpToolset`**: Connects to an MCP server, discovers its available tools, and provides them to your agent.
- **`McpSessionManager`**: Manages the underlying client sessions (Stdio or HTTP).
- **`McpTool`**: Wraps an individual MCP tool so it behaves like a standard ADK `IBaseTool`.

## Example: Connecting to a Stdio MCP Server

You can connect to an MCP server that runs as a local process via standard input/output (stdio).

```csharp
using GoogleAdk.Tools.Mcp;

// Connect to a local npx-based MCP server
var toolset = new McpToolset(new StdioConnectionParams
{
    Command = "npx",
    Arguments = ["-y", "@modelcontextprotocol/server-everything"]
});

var agent = new LlmAgent(new LlmAgentConfig
{
    Name = "mcp_agent",
    ModelName = "gemini-2.5-flash",
    // Add the entire discovered toolset to the agent
    Toolsets = new List<BaseToolset> { toolset }
});

// Remember to dispose the toolset when shutting down to close the process
// await toolset.DisposeAsync();
```

## Example: Blender MCP (uvx + stdio)

If you're running the Blender MCP server via `uvx`, configure the toolset like this:

```csharp
using GoogleAdk.Tools.Mcp;

var blenderToolset = new McpToolset(new StdioConnectionParams
{
    Command = "uvx",
    Arguments = ["-q", "blender-mcp"],
    TimeoutMs = 60_000
});
```

## Example: Connecting to an HTTP/SSE MCP Server

You can also connect to remote MCP servers over HTTP/SSE.

```csharp
using GoogleAdk.Tools.Mcp;

var toolset = new McpToolset(new HttpConnectionParams
{
    Url = "http://localhost:8788/mcp"
});

var agent = new LlmAgent(new LlmAgentConfig
{
    Name = "mcp_http_agent",
    ModelName = "gemini-2.5-flash",
    Toolsets = new List<BaseToolset> { toolset }
});
```

## Placeholders

- Authentication and connection hardening: _coming soon_
