# Google Search Tool

!!! warning "Gemini Only"
    This tool relies on Google's Search Grounding API and is exclusively compatible with Gemini models (e.g., `gemini-2.5-flash`).

The `GoogleSearchTool` provides agents with real-time web search capabilities to answer factual, up-to-date questions.

## Overview

Unlike standard function tools, this tool signals the Gemini model to activate its internal search capabilities. The model decides when to search, retrieves the results, synthesizes an answer, and returns grounding metadata containing the exact search queries used and URLs cited.

## Usage

Instantiate `GoogleSearchTool` and add it to your agent's `Tools` list.

```csharp
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Tools;

var agent = new LlmAgent(new LlmAgentConfig
{
    Name = "search_agent",
    Model = "gemini-2.5-flash",
    Instruction = "You are a research assistant. Use Google Search for current events.",
    Tools = [new GoogleSearchTool()]
});
```

### Accessing Grounding Metadata

When the agent searches, the response events will contain `GroundingMetadata` detailing the queries run.

```csharp
await foreach (var evt in runner.RunAsync("user-id", "session-id", userMessage))
{
    if (evt.GroundingMetadata?.WebSearchQueries != null)
    {
        Console.WriteLine("Search Queries Run:");
        foreach (var query in evt.GroundingMetadata.WebSearchQueries)
        {
            Console.WriteLine($"- {query}");
        }
    }
}
```