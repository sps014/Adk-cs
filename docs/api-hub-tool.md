# API Hub Tool

The `ApiHubTool` integrates your agent with Google Cloud API Hub, allowing the LLM to search for, discover, and understand enterprise APIs registered within your organization.

## Usage

Ensure your application is authenticated and has permissions to read from the API Hub in the target project.

```csharp
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Tools;

var agent = new LlmAgent(new LlmAgentConfig
{
    Name = "api_discoverer",
    Model = "gemini-2.5-flash",
    Instruction = "You help developers find internal APIs. Use the API Hub tool to search for enterprise APIs.",
    Tools = [ new ApiHubTool() ]
});
```

### LLM Parameters
The LLM invokes this tool with:
- `projectId` (string, required) 
- `location` (string, required) - e.g., "global".
- `searchQuery` (string, required) - e.g., "payments api".

The tool returns a list of matching APIs and their descriptions for the LLM to synthesize.