# Vertex AI Search Tool

The `VertexAiSearchTool` (also known as the Discovery Engine Search Tool) connects your agent directly to Google Cloud's Vertex AI Search (formerly Discovery Engine). It enables the agent to execute searches against your enterprise datastores and document collections.

## Overview

If you have ingested corporate documents, wikis, or product catalogs into a Vertex AI Search Data Store, this tool allows the LLM to search those records dynamically to answer user questions using Retrieval-Augmented Generation (RAG) paradigms. 

## Usage

To configure the tool, you must provide the full Google Cloud Resource ID for your Data Store or Engine.

### Resource ID Format
The datastore ID typically follows this format:
`projects/{project_id}/locations/{location}/collections/default_collection/dataStores/{datastore_id}`

### Implementation

```csharp
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Tools;

var projectId = "my-gcp-project";
var location = "global";
var datastoreId = "my-internal-docs-ds";
var fullDataStoreId = $"projects/{projectId}/locations/{location}/collections/default_collection/dataStores/{datastoreId}";

var vertexSearchTool = new VertexAiSearchTool(dataStoreId: fullDataStoreId);

var agent = new LlmAgent(new LlmAgentConfig
{
    Name = "enterprise_search_agent",
    Model = "gemini-2.5-pro",
    Instruction = "You are an internal IT assistant. Always use the Vertex AI Search tool to find answers in the internal documentation.",
    Tools = [ vertexSearchTool ]
});
```

## Grounding Metadata

Similar to the standard Google Search tool, responses augmented by Vertex AI Search will include `GroundingMetadata` on the output events. You can inspect this metadata to see exactly which documents were retrieved and cited by the model.

```csharp
await foreach (var evt in runner.RunAsync("user-id", "session-id", userMessage))
{
    // Print the model's text response...
    
    // Inspect what was searched in Vertex AI:
    if (evt.GroundingMetadata?.WebSearchQueries?.Count > 0)
    {
        Console.WriteLine("Searched Vertex AI for:");
        foreach (var query in evt.GroundingMetadata.WebSearchQueries)
        {
            Console.WriteLine($"- {query}");
        }
    }
}
```

## Example

For a complete working example, see the [Vertex AI Search Sample](../samples/GoogleAdk.Samples.VertexAiSearch) in the repository.