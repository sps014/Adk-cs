# Vertex AI RAG Retrieval Tool

!!! warning "Gemini & Vertex AI Only"
    This tool leverages Google Cloud's Retrieval Config API and is exclusively compatible with Gemini models on Vertex AI.

The `VertexAiRagRetrievalTool` integrates deeply with Google Cloud's Vertex AI to perform Retrieval-Augmented Generation (RAG). 

## Overview

Unlike standard function-calling tools, this tool leverages the `RetrievalConfig` of the Gemini generation API. The Gemini backend automatically queries your specified Vertex AI Data Stores or RAG Corpora *during* text generation, returning rich `GroundingMetadata`.

## Usage

Configure the tool by specifying data stores or corpora for the backend to retrieve from:

```csharp
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Tools;

var ragTool = new VertexAiRagRetrievalTool(
    ragResources: [
        new VertexAiSearchDataStoreSpec { DataStore = "projects/my-project/locations/global/collections/default_collection/dataStores/my-docs" }
    ],
    // ragCorpora: ["projects/my-project/locations/us-central1/ragCorpora/1234567890"],
    similarityTopK: 5,
    vectorDistanceThreshold: 0.3
);

var agent = new LlmAgent(new LlmAgentConfig
{
    Name = "rag_agent",
    Model = "gemini-2.5-pro",
    Instruction = "Answer based on the provided RAG context.",
    Tools = [ragTool]
});
```