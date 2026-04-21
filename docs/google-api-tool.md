# Google API Tool

The `GoogleApiTool` acts as a dynamic bridge to virtually any Google Cloud API via the Google Discovery Service. 

## Overview

Instead of hardcoding hundreds of specific tools for every Google Cloud service (e.g., Compute Engine, Cloud Storage, Vision API), the `GoogleApiTool` leverages the Google API Discovery Service. When the LLM calls this tool, it specifies the API name and version. The tool dynamically fetches the API schema, structures the request, and executes the REST call.

## Usage

This is a highly advanced tool that requires an LLM with strong reasoning capabilities, as the LLM must understand the structure of the API it intends to call.

```csharp
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Tools;

var agent = new LlmAgent(new LlmAgentConfig
{
    Name = "gcp_admin_agent",
    Model = "gemini-2.5-pro",
    Instruction = "You are a GCP cloud administrator. Use the GoogleApiTool to manage cloud resources dynamically when asked.",
    Tools = [ new GoogleApiTool() ]
});
```

### Parameters passed by the LLM

When invoking this tool, the LLM will provide:
- `apiName` (string, required) - e.g., "compute", "storage", "vision".
- `apiVersion` (string, required) - e.g., "v1".
- `resource` (string, required) - The resource collection (e.g., "instances").
- `method` (string, required) - The method to call (e.g., "list", "get", "insert").
- `parameters` (object, optional) - Query string or route parameters (like `project`, `zone`).
- `requestBody` (object, optional) - The JSON payload for POST/PUT requests.

### Example LLM Flow

If a user says *"List my compute engine instances in us-central1-a for project my-project"*, the LLM might generate a tool call like:

```json
{
  "apiName": "compute",
  "apiVersion": "v1",
  "resource": "instances",
  "method": "list",
  "parameters": {
    "project": "my-project",
    "zone": "us-central1-a"
  }
}
```

The tool will execute the GET request and return the JSON response of instances back to the model.