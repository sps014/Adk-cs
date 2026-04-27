# Getting Started

The Agent Development Kit (ADK) for .NET provides foundational components to build, test, and deploy intelligent agents.

## Prerequisites

- **.NET SDK 10.0+**
- **API Key** (e.g., Google AI Studio, Vertex AI)

### Authentication

Set the required environment variables in a `.env` file at the root of your project. The ADK automatically loads them via `AdkEnv.Load()`.

**Google AI Studio**
```env
GOOGLE_API_KEY=your_api_key_here
```

**Vertex AI**
```env
GOOGLE_GENAI_USE_VERTEXAI=True
GOOGLE_CLOUD_PROJECT=your_project_id
GOOGLE_CLOUD_LOCATION=us-central1
```

## First Agent in 30 Seconds

1. Create a new console app and install the package:
```bash
dotnet new console -n QuickstartAgent
cd QuickstartAgent
dotnet add package GoogleAdk --prerelease
```

2. Update `Program.cs`:
```csharp
using GoogleAdk.Core;
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Runner;

AdkEnv.Load();

var agent = new LlmAgent(new LlmAgentConfig
{
    Name = "quickstart",
    Model = "gemini-2.5-flash",
    Instruction = "You are a concise, helpful assistant."
});

await ConsoleRunner.RunAsync(agent);
```

## Running the Web UI

To test and debug your agents visually, you can use the built-in server:

```csharp
await AdkServer.RunAsync(agent);
```
[Learn more about ADK Web](adk-web.md)

## Custom Runners

For deep integration into production apps, background services, or Discord bots, you can bypass the pre-built runners and construct the core `Runner` directly:

```csharp
var runner = new InMemoryRunner("custom-app", agent);

await foreach (var evt in runner.RunAsync("user-1", "session-id", content))
{
    // Handle your own streams and events
}
```
[Learn more about Custom Runners](custom-runner.md)