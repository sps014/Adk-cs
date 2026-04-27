# Google API Tool

The `GoogleApiTool` acts as a dynamic bridge to virtually any Google Cloud API (e.g., Compute Engine, Cloud Storage) via the Google API Discovery Service. 

## Usage

This advanced tool allows the LLM to dynamically fetch API schemas, structure requests, and execute REST calls without hardcoding specific tools.

```csharp
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Tools;

var agent = new LlmAgent(new LlmAgentConfig
{
    Name = "gcp_admin_agent",
    Model = "gemini-2.5-pro",
    Instruction = "You are a GCP cloud administrator. Use the GoogleApiTool to manage cloud resources dynamically.",
    Tools = [ new GoogleApiTool() ]
});
```

### Example Flow

If a user asks *"List my compute engine instances in us-central1-a for project my-project"*, the LLM invokes the tool with parameters like:
- `apiName`: "compute"
- `apiVersion`: "v1"
- `resource`: "instances"
- `method`: "list"
- `parameters`: `{ "project": "my-project", "zone": "us-central1-a" }`

The tool executes the request and returns the JSON response.