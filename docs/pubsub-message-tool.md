# Pub/Sub Message Tool

The `PubSubMessageTool` allows your agents to act as event emitters by publishing messages to Google Cloud Pub/Sub topics.

## Overview

While most tools are used to *read* data, the `PubSubMessageTool` is an action-oriented tool. It allows the LLM to trigger downstream asynchronous processes, orchestrate workflows, or notify other microservices in an event-driven architecture.

## Prerequisites

Your application must be authenticated with Google Cloud and have the `roles/pubsub.publisher` IAM role for the target topics.

## Usage

Instantiate the tool and instruct your agent on when to publish messages.

```csharp
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Tools;

var agent = new LlmAgent(new LlmAgentConfig
{
    Name = "automation_agent",
    Model = "gemini-2.5-flash",
    Instruction = @"
        You are an infrastructure automation bot. 
        If a user asks to trigger a new build, publish a message to the Pub/Sub topic.
        Use projectId 'my-project' and topicId 'build-triggers'.
        Format the message as a JSON string containing the requested repository name.",
    Tools = [ new PubSubMessageTool() ]
});
```

### Parameters passed by the LLM

When the agent uses this tool, it will generate a function call with the following arguments:
- `projectId` (string, required) - The Google Cloud project.
- `topicId` (string, required) - The name of the Pub/Sub topic.
- `message` (string, required) - The payload of the message. This can be plain text or a JSON-serialized string depending on your instructions.

### Feedback
The tool returns the `MessageId` upon successful publication, allowing the LLM to confirm to the user that the event was successfully dispatched.