# API Hub Tool

The `ApiHubTool` integrates your agent with Google Cloud API Hub, allowing the LLM to search for, discover, and understand enterprise APIs registered within your organization.

## Overview

In large enterprises, developers and AI agents often struggle to find internal APIs. Google Cloud API Hub acts as a centralized registry. By providing the `ApiHubTool`, your LLM agent can search this registry based on natural language queries, retrieving API specs, endpoints, and descriptions.

## Usage

Your application must be authenticated and have permissions to read from the API Hub in the target project.

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

### Parameters passed by the LLM

The LLM invokes this tool with:
- `projectId` (string, required) - Your Google Cloud project ID.
- `location` (string, required) - The location of the API Hub (e.g., "global", "us-central1").
- `searchQuery` (string, required) - The search terms (e.g., "payments api", "user management").

### Output

The tool queries the API Hub and returns a list of matching APIs, including their names, descriptions, and potentially links to their OpenAPI specifications. The LLM synthesizes this information to tell the user which internal API they should use for their task.