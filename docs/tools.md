# Tools Overview

Tools provide your agents with the ability to interact with the outside world, fetch data, and perform actions. 

## Built-in Tools

The ADK includes several built-in tools. For detailed usage, check their individual documentation pages:

| Tool Category | Available Tools |
| :--- | :--- |
| **Search & AI** | `GoogleSearchTool`, `VertexAiSearchTool`, `DiscoveryEngineSearchTool`, `VertexAiRagRetrievalTool` |
| **Databases** | `BigQueryQueryTool`, `BigQueryMetadataTool`, `SpannerQueryTool`, `SpannerSearchTool`, `BigtableQueryTool` |
| **Cloud Services** | `PubSubMessageTool`, `GoogleApiTool`, `ApiHubTool` |
| **Agent & OS** | `AgentTool` (sub-agents), `ComputerUseTool` (browser/desktop automation), `AuthTool` |

> See the **Tools & Integrations** sidebar for deep dives into configuring and using each of these built-in tools.

## Source Generated Tools (Recommended)

The most robust way to create tools in C# is by using the `[FunctionTool]` attribute. The ADK's source generators automatically parse your XML documentation and method signature to create the JSON schema required by the LLM.

```csharp
public static partial class WeatherTools
{
    /// <summary>Gets the current weather for a specified city.</summary>
    /// <param name="city">The name of the city (e.g., "Seattle").</param>
    [FunctionTool]
    public static object? GetWeather(string city)
    {
        return new { city, temperature = 22, condition = "Sunny" };
    }
}

// Attach the generated tool to your agent
var agent = new LlmAgent(new LlmAgentConfig
{
    Name = "weather_agent",
    Model = "gemini-2.5-flash",
    Tools = [WeatherTools.GetWeatherTool]
});
```

## Toolsets

`BaseToolset` allows for dynamic, runtime resolution of tool collections.

### OpenAPI Toolset
Automatically generate tools from an OpenAPI/Swagger specification string or file:

```csharp
using GoogleAdk.Tools.OpenApi;

// Exposes all operations defined in the swagger spec as callable tools
var toolset = new OpenAPIToolset(openApiSpecString, "json");
```

### Agent Toolset (Hierarchical)
Wrap another configured agent as a tool. When the primary LLM needs specialized expertise, it delegates the task to the sub-agent.

```csharp
var coordinator = new LlmAgent(new LlmAgentConfig
{
    Name = "coordinator",
    Model = "gemini-2.5-flash",
    Tools = [new AgentTool(specializedSubAgent)]
});
```