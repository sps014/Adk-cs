# Google Search Tool

The `GoogleSearchTool` provides agents with real-time web search capabilities using Google Search Grounding. When this tool is attached, the LLM can dynamically search the web to answer factual, up-to-date, or real-time questions.

## Overview

Unlike standard function tools that execute local code, the `GoogleSearchTool` signals the Google AI models (like `gemini-2.5-flash`) to activate their internal Google Search capabilities. The model decides when a search is necessary, retrieves the search results, synthesizes an answer, and returns grounding metadata containing the exact search queries used and the URLs cited.

## Usage

To use this tool, simply instantiate `GoogleSearchTool` and add it to your agent's `Tools` list.

```csharp
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Tools;

var searchAgent = new LlmAgent(new LlmAgentConfig
{
    Name = "search_agent",
    Model = "gemini-2.5-flash",
    Instruction = "You are a helpful research assistant. When asked about current events or factual questions, use Google Search to find accurate information.",
    Tools = [ new GoogleSearchTool() ]
});
```

### Accessing Grounding Metadata

When the agent uses the search tool, the response events will contain `GroundingMetadata`. This allows you to inspect what queries the model ran and see any entry points it rendered.

```csharp
await foreach (var evt in runner.RunAsync("user-id", "session-id", userMessage))
{
    if (evt.Content?.Parts != null && evt.Partial != true)
    {
        Console.WriteLine(evt.Content.Parts.FirstOrDefault()?.Text);
    }

    if (evt.GroundingMetadata != null)
    {
        Console.WriteLine("Search Queries Run:");
        foreach (var query in evt.GroundingMetadata.WebSearchQueries ?? new List<string>())
        {
            Console.WriteLine($"- {query}");
        }
    }
}
```

## Requirements

When using the `GoogleSearchTool` via Vertex AI (`GOOGLE_GENAI_USE_VERTEXAI=True`), ensure your Google Cloud Project has the required APIs enabled for Grounding.

## Example

For a complete working example, see the [Google Search Sample](../samples/GoogleAdk.Samples.GoogleSearch) in the repository.