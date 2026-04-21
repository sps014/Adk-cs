# Agent Tool

The `AgentTool` is a unique wrapper in the `GoogleAdk.Core.Tools` namespace that allows you to treat an entire `BaseAgent` (or `LlmAgent`) as a callable tool. This is the cornerstone of **hierarchical agent orchestration**.

## Overview

By wrapping a specialized agent in an `AgentTool`, a "coordinator" or "manager" agent can delegate complex tasks to sub-agents. The LLM will decide to invoke the sub-agent just like any other function call.

## Usage

Instantiate your specialized sub-agent, wrap it in the `AgentTool`, and add it to the `Tools` collection of the parent agent.

```csharp
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Tools;

// 1. Create a specialized sub-agent
var codingAgent = new LlmAgent(new LlmAgentConfig
{
    Name = "coder",
    Model = "gemini-2.5-pro",
    Instruction = "You write pristine C# code based on user requirements. Do not provide explanations, only code."
});

// 2. Wrap it as a tool
var codingTool = new AgentTool(codingAgent);

// 3. Create the coordinator agent
var coordinatorAgent = new LlmAgent(new LlmAgentConfig
{
    Name = "coordinator",
    Model = "gemini-2.5-flash",
    Instruction = "You are a tech lead. If the user asks for code, use the 'coder' tool to generate it, then review it before answering.",
    Tools = [ codingTool ]
});
```

## How It Works

1. The coordinator LLM determines it needs to generate code and calls the `coder` tool with a text prompt payload.
2. The ADK intercepts this tool call.
3. The ADK spins up an ephemeral runner for the `coder` agent, passing the LLM's prompt as the `UserContent`.
4. The `coder` agent runs to completion.
5. The `coder` agent's final response is packaged into a JSON object and returned as the tool response back to the coordinator.
6. The coordinator agent resumes its thinking process with the code provided by the sub-agent.

## Example

For a complete working example, see the [SubAgents Sample](../samples/GoogleAdk.Samples.SubAgents) in the repository.