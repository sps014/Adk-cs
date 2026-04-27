# Features & Architecture

The ADK provides a robust, composable architecture designed for complex agentic workflows. Build logic efficiently without rewriting boilerplate.

## Agent Types

Built-in orchestration patterns determine how a sequence of actions executes:

- **`LlmAgent`**: The foundational agent. Handles tool execution, context windows, code execution, and system instructions.
- **`SequentialAgent`**: Executes sub-agents sequentially, chaining outputs as inputs.
- **`ParallelAgent`**: Runs sub-agents concurrently with identical context, aggregating responses.
- **`LoopAgent`**: Runs a sub-agent recursively until a condition is met or an escalation occurs.

## Processors Pipeline

`LlmAgent` uses a highly customizable pipeline to mutate requests and responses before and after LLM dispatch.

| Processor | Purpose |
| :--- | :--- |
| `InstructionsLlmRequestProcessor` | Injects system instructions and few-shot examples. |
| `CodeExecutionRequestProcessor` | Enables agents to write and execute code. |
| `RequestConfirmationLlmRequestProcessor` | Pauses for human confirmation on sensitive actions. |
| `ContextCacheRequestProcessor` | Caches prompts automatically to lower costs. |
| `OutputSchemaRequestProcessor` | Forces output to match a strict JSON schema. |

### Customizing Pipelines

Override the default sequence to inject proprietary logic or logging:

```csharp
var agent = new LlmAgent(new LlmAgentConfig
{
    Name = "custom_pipeline_agent",
    Model = "gemini-2.5-flash",
    RequestProcessors = [
        BasicLlmRequestProcessor.Instance,
        new MyCustomProcessor(),
        ContentRequestProcessor.Instance
    ]
});
```

## Context Compaction

Prevent context window overflows during long-running flows using automatic compactors.

| Compactor | Strategy |
| :--- | :--- |
| **Truncation** | FIFO - Removes oldest messages. |
| **Token** | Evaluates tokens and limits payload size. |
| **Summarization** | Uses the LLM to summarize and replace long dialogue blocks. |

```csharp
var agent = new LlmAgent(new LlmAgentConfig
{
    Name = "compact_agent",
    Model = "gemini-2.5-flash",
    ContextCompactors = [new TruncatingContextCompactor(2000)] // Keep last 2000 tokens
});
```