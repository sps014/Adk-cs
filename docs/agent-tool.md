# Agent Tool

The `AgentTool` allows you to treat an entire `BaseAgent` as a callable tool, forming the basis of **hierarchical agent orchestration**.

## Overview

By wrapping a specialized sub-agent in an `AgentTool`, a "coordinator" agent can delegate complex tasks. The LLM decides to invoke the sub-agent just like any other function.

## Usage

Instantiate a sub-agent, wrap it, and add it to the parent agent's `Tools` list.

```csharp
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Tools;

// 1. Create specialized sub-agent
var codingAgent = new LlmAgent(new LlmAgentConfig
{
    Name = "coder",
    Model = "gemini-2.5-pro",
    Instruction = "Write pristine C# code. Do not explain, only code."
});

// 2. Create the coordinator
var coordinatorAgent = new LlmAgent(new LlmAgentConfig
{
    Name = "coordinator",
    Model = "gemini-2.5-flash",
    Instruction = "You are a tech lead. Use the 'coder' tool to generate code, then review it.",
    Tools = [ new AgentTool(codingAgent) ]
});
```

## Flow

1. The coordinator decides it needs code and calls `coder`.
2. The ADK spins up an ephemeral runner for `coder` with the prompt.
3. The `coder` agent runs to completion.
4. The final response is returned to the coordinator as JSON.
5. The coordinator resumes execution with the code.