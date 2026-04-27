# Pub/Sub Message Tool

The `PubSubMessageTool` allows your agents to publish messages to Google Cloud Pub/Sub topics, enabling them to trigger downstream asynchronous workflows or notify microservices.

## Usage

Ensure your application has the `roles/pubsub.publisher` IAM role for the target topics.

```csharp
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Tools;

var agent = new LlmAgent(new LlmAgentConfig
{
    Name = "automation_agent",
    Model = "gemini-2.5-flash",
    Instruction = "If a user asks to trigger a build, publish a message to the 'build-triggers' topic in project 'my-project'.",
    Tools = [ new PubSubMessageTool() ]
});
```

### LLM Parameters
The agent will automatically provide:
- `projectId` (string, required) 
- `topicId` (string, required) 
- `message` (string, required) - The payload of the message.

The tool returns the `MessageId` upon successful publication.