# Model Context Protocol (MCP)

The ADK provides first-class support for the Model Context Protocol (MCP), enabling your agents to dynamically discover and consume tools provided by external MCP servers. This allows your agent to safely interact with a vast ecosystem of third-party APIs and local resources.

## Core Components

- **`McpToolset`**: Connects to an MCP server, discovers its available tools, and makes them available to an agent. It manages the lifecycle of the connection.
- **`McpSessionManager`**: Manages the underlying client sessions (e.g., Stdio or HTTP) and handles protocol-level communication.
- **`McpTool`**: A dynamically generated wrapper for an individual MCP tool, exposing it to the LLM identically to any other `IBaseTool`.

## Connecting to an MCP Server

The ADK supports connecting to MCP servers via standard input/output (stdio) or over HTTP/SSE.

### Stdio Connections (Local Processes)

Connecting to a local MCP server via stdio is ideal for utilities running on the same machine, such as `npx` packages or Python tools via `uvx`.

```csharp
using GoogleAdk.Tools.Mcp;
using GoogleAdk.Core.Agents;

// 1. Configure the connection parameters for the MCP server
var stdioParams = new StdioConnectionParams
{
    Command = "npx",
    Arguments = ["-y", "@modelcontextprotocol/server-everything"],
    TimeoutMs = 30_000 // Optional: Set a startup/request timeout
};

// 2. Initialize the toolset
// Note: It's important to dispose of the toolset when shutting down 
// your application to ensure the external process exits cleanly.
var toolset = new McpToolset(stdioParams, prefix: "mcp");

// 3. Attach the toolset to your agent
var agent = new LlmAgent(new LlmAgentConfig
{
    Name = "mcp_agent",
    ModelName = "gemini-2.5-flash",
    Instruction = "You have access to MCP tools. Use them to assist the user.",
    // The toolset will automatically discover all tools provided by the server
    Toolsets = [toolset] 
});

// To clean up later:
// await toolset.DisposeAsync();
```

**Using Python-based MCP Servers (e.g., uvx):**

```csharp
var blenderParams = new StdioConnectionParams
{
    Command = "uvx",
    Arguments = ["-q", "blender-mcp"],
    TimeoutMs = 60_000
};
var blenderToolset = new McpToolset(blenderParams);
```

### HTTP/SSE Connections (Remote Servers)

You can connect to remote MCP servers over HTTP using Server-Sent Events (SSE). This is useful when the MCP server is hosted externally or behind a load balancer.

```csharp
using GoogleAdk.Tools.Mcp;
using GoogleAdk.Core.Agents;

// 1. Configure the connection parameters for the remote server
var httpParams = new HttpConnectionParams
{
    Url = "http://localhost:8788/mcp",
    // You can also provide custom HTTP headers if the server requires them
    Headers = new Dictionary<string, string>
    {
        { "Authorization", "Bearer my-secret-token" }
    }
};

// 2. Initialize the toolset
var toolset = new McpToolset(httpParams);

// 3. Attach the toolset to your agent
var agent = new LlmAgent(new LlmAgentConfig
{
    Name = "mcp_http_agent",
    ModelName = "gemini-2.5-flash",
    Toolsets = [toolset]
});
```

## Prefixing Tool Names

MCP servers often expose generic tool names like `search` or `execute`. To prevent naming collisions when using multiple toolsets or built-in tools, you can specify a prefix.

```csharp
// Tools will be exposed as 'github_search', 'github_execute', etc.
var toolset = new McpToolset(stdioParams, prefix: "github");
```

## Security and Authentication

- The `Headers` property in `HttpConnectionParams` allows you to pass authentication tokens to remote servers.
- Further connection hardening and granular tool filtering capabilities will be added in future releases.
