# Discovery Engine Search Tool

The `DiscoveryEngineSearchTool` (often used interchangeably with `VertexAiSearchTool`) provides access to Google Cloud's Discovery Engine. It allows agents to perform semantic and keyword searches over indexed document collections.

## Overview

Unlike standard SQL databases, Discovery Engine datastores index PDFs, HTML pages, and unstructured text. By attaching this tool, your agent gains the ability to "read" your corporate knowledge base and answer user questions grounded in your proprietary documents.

## Usage

You must provide the explicit resource name of the Datastore or Engine you wish the agent to search.

### Resource Formats
You can initialize the tool using either a `datastore` or an `engine` resource ID:
- Datastore: `projects/{project}/locations/{location}/collections/default_collection/dataStores/{datastore_id}`
- Engine: `projects/{project}/locations/{location}/collections/default_collection/engines/{engine_id}`

### Implementation

```csharp
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Tools;

var datastoreId = "projects/my-project/locations/global/collections/default_collection/dataStores/hr-docs";
var searchTool = new DiscoveryEngineSearchTool(datastore: datastoreId);

var hrAgent = new LlmAgent(new LlmAgentConfig
{
    Name = "hr_assistant",
    Model = "gemini-2.5-flash",
    Instruction = "You are an HR assistant. Use the search tool to find policies in the HR datastore.",
    Tools = [ searchTool ]
});
```

## How It Differs From RAG Tools

While this tool performs a search and returns snippets to the LLM (which the LLM then reads as part of its conversation history), the `VertexAiRagRetrievalTool` uses a specialized API that integrates the retrieval step directly into the generation step on the model's backend. 

Use `DiscoveryEngineSearchTool` when you want the agent to explicitly decide *when* and *what* to search via standard function calling.