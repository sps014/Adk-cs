# Discovery Engine Search Tool

!!! warning "Gemini & Vertex AI Only"
    This tool leverages Google Cloud's Discovery Engine APIs and is designed for Gemini models running via Vertex AI.

The `DiscoveryEngineSearchTool` (often used interchangeably with `VertexAiSearchTool`) provides access to Google Cloud's Discovery Engine to perform semantic searches over indexed PDFs, HTML, and unstructured text.

## Usage

Initialize the tool using either a `datastore` or an `engine` resource ID:

- **Datastore**: `projects/{project}/locations/{location}/collections/default_collection/dataStores/{datastore_id}`
- **Engine**: `projects/{project}/locations/{location}/collections/default_collection/engines/{engine_id}`

```csharp
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Tools;

var datastoreId = "projects/my-project/locations/global/collections/default_collection/dataStores/hr-docs";

var hrAgent = new LlmAgent(new LlmAgentConfig
{
    Name = "hr_assistant",
    Model = "gemini-2.5-flash",
    Instruction = "You are an HR assistant. Find policies in the HR datastore.",
    Tools = [new DiscoveryEngineSearchTool(datastore: datastoreId)]
});
```

## Vs. RAG Retrieval Tool

Use `DiscoveryEngineSearchTool` when you want the agent to explicitly decide *when* and *what* to search via a standard function call. Use `VertexAiRagRetrievalTool` to integrate the retrieval directly into the backend generation step.