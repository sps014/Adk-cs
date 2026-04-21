# Vertex AI RAG Retrieval Tool

The `VertexAiRagRetrievalTool` integrates deeply with Google Cloud's Vertex AI to perform Retrieval-Augmented Generation (RAG). 

## Overview

Unlike standard function-calling tools (where the LLM requests a search, waits for the application to perform it, and then reads the result), the `VertexAiRagRetrievalTool` leverages the `RetrievalConfig` of the Gemini generation API. 

When you attach this tool, you are instructing the Gemini backend to automatically query your specified Vertex AI Data Stores or RAG Corpora *during* the text generation process.

## Usage

You configure the tool by specifying exactly which data stores or corpora the backend should retrieve from, along with similarity parameters.

```csharp
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Tools;

// Define the data store to retrieve from
var ragResources = new List<VertexAiSearchDataStoreSpec>
{
    new VertexAiSearchDataStoreSpec 
    { 
        DataStore = "projects/my-project/locations/global/collections/default_collection/dataStores/my-docs" 
    }
};

// Or define a RAG Corpora
var corpora = new List<string> 
{ 
    "projects/my-project/locations/us-central1/ragCorpora/1234567890" 
};

// Initialize the tool
var ragTool = new VertexAiRagRetrievalTool(
    ragResources: ragResources,
    ragCorpora: corpora,
    similarityTopK: 5,
    vectorDistanceThreshold: 0.3
);

var agent = new LlmAgent(new LlmAgentConfig
{
    Name = "rag_agent",
    Model = "gemini-2.5-pro",
    Instruction = "You are a knowledgeable assistant. Answer based on the provided RAG context.",
    Tools = [ ragTool ]
});
```

## How It Works

1. **Configuration:** The `VertexAiRagRetrievalTool` does not implement `RunAsync`. Instead, when the ADK builds the `LlmRequest` payload, this tool populates the `GenerateContentConfig.Tools` array with a `Retrieval` object.
2. **Backend Execution:** When the prompt is sent to Vertex AI, Google's servers execute the retrieval against your data store, augment the prompt with the found snippets, and generate the final answer.
3. **Grounding:** Because the retrieval happens on the backend, the response will automatically contain rich `GroundingMetadata`, pointing exactly to the chunks of text or URIs that were utilized to formulate the answer.