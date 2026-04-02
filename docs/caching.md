# Prompt Caching (Context Caching)

Large language models charge for the number of tokens processed. In use cases like code analysis or iterative questioning over huge documents, sending the identical, massive context payload repeatedly incurs significant latency and token costs.

The ADK leverages **Context Caching** (currently supported natively by Gemini models) to cache massive contexts securely server-side.

## Enabling Prompt Caching

To enable prompt caching for an agent, simply provide a `ContextCacheConfig` to your `LlmAgentConfig`. The `ContextCacheRequestProcessor` will dynamically evaluate the conversational history. If the context meets the caching criteria, it builds a cache request; if the context was recently cached, the model leverages the stored payload without reprocessing the tokens.

```csharp
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Context;
using GoogleAdk.Models.Gemini;

var agent = new LlmAgent(new LlmAgentConfig
{
    Name = "cache_agent",
    ModelName = "gemini-1.5-pro", // Caching is most valuable on large reasoning models
    
    // Enables the ContextCacheRequestProcessor
    ContextCacheConfig = new ContextCacheConfig
    {
        // Add configurations here based on provider implementations
        // Note: Different model providers implement caching TTLs differently.
    }
});
```

**Benefits of Context Caching:**
- **Cost Reduction**: Reused context blocks are typically billed at a fraction of the cost.
- **Latency Optimization**: Substantially reduces time-to-first-byte (TTFB) since the model circumvents re-tokenizing massive prefix prompts or documents.
- **Improved Performance on Repetitive Queries**: Highly effective for applications building "Chat with PDF" workflows or complex data extraction pipelines.