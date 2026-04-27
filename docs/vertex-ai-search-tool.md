# Vertex AI Search Tool

!!! warning "Gemini & Vertex AI Only"
    This tool leverages Google Cloud's Discovery Engine APIs and is designed for Gemini models running via Vertex AI.

The `VertexAiSearchTool` (Discovery Engine Search Tool) connects your agent directly to Google Cloud's Vertex AI Search. It enables the agent to execute searches against your enterprise datastores and document collections.

## Usage

You must provide the full Google Cloud Resource ID for your Data Store or Engine.

**Resource ID Format:**
`projects/{project_id}/locations/{location}/collections/default_collection/dataStores/{datastore_id}`

```csharp
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Tools;

var fullDataStoreId = "projects/my-project/locations/global/collections/default_collection/dataStores/my-docs";
var vertexSearchTool = new VertexAiSearchTool(dataStoreId: fullDataStoreId);

var agent = new LlmAgent(new LlmAgentConfig
{
    Name = "enterprise_search_agent",
    Model = "gemini-2.5-pro",
    Instruction = "You are an internal IT assistant. Use Vertex AI Search to find answers.",
    Tools = [vertexSearchTool]
});
```

### Grounding Metadata

Responses augmented by Vertex AI Search include `GroundingMetadata` on the output events, allowing you to see exactly which documents were retrieved and cited.