# Running Agents Overview

Once an agent is configured, it must be executed within a runtime environment that manages session state, artifacts, memory, and telemetry. The ADK uses the `Runner` concept for execution.

The ADK provides several built-in runners and environments depending on your use case:

- **[ADK Web](adk-web.md)**: A rich, interactive visual dashboard with an ASP.NET Core server and Blazor front-end. Ideal for debugging and visualizing complex agent interactions.
- **[ADK Server A2A](adk-server-a2a.md)**: Exposes HTTP APIs (`/run`, `/run_sse`, etc.) for synchronous, streaming, and live execution, as well as Agent-to-Agent capabilities.
- **[Console Runner](console-runner.md)**: A beautiful, interactive terminal experience built on `Spectre.Console` for rapid CLI development and testing.
- **[Custom Runner](custom-runner.md)**: Instructions on how to instantiate the core `Runner` class directly for deep integration into production apps like Discord bots or background services.

## The Core `Runner` Concept

At its heart, execution is managed by the core `Runner` class, which orchestrates the flow of events between the agent, the user, and the persistent storage services.

A basic invocation requires:
1. A configured `Runner` with storage services (Sessions, Artifacts, Memory).
2. A unique `SessionId` tied to a `UserId` and `AppName`.
3. An input payload (`Content`).

```csharp
// Bootstraps a runner with all required InMemory services for quick testing
var runner = new InMemoryRunner("cli-app", myAgent);

// Explicitly create or fetch a Session
var session = await runner.SessionService.CreateSessionAsync(new CreateSessionRequest
{
    AppName = "cli-app",
    UserId = "dev-user"
});

// Run the invocation and process the event stream
await foreach (var evt in runner.RunAsync("dev-user", session.Id, userMessage))
{
    // handle event output
}
```

## Advanced Run Configurations

Regardless of which runner you use, the `RunConfig` object provides fine-grained control over execution behavior, multimodality, live sessions, and context limits. You can inject this configuration when invoking the runner or setting up the runner context.

```csharp
var runConfig = new RunConfig
{
    // Limits total LLM calls per run to prevent runaway recursion
    MaxLlmCalls = 10,
    
    // Halts execution on any tool call, allowing client-side execution
    PauseOnToolCalls = true,
    
    // Configures audio response and speech configurations
    ResponseModalities = ["AUDIO"],
    SpeechConfig = new SpeechConfig(),
    
    // Allows saving bidirectional audio/video payloads into session storage
    SaveLiveBlob = true,
    
    // Automatically fetch recent session events upon start
    GetSessionConfig = new GetSessionConfig { NumRecentEvents = 50 },
    
    // Automatically save inline input file payloads to the artifact service
    SaveInputBlobsAsArtifacts = true
};
```

## Persisting Sessions with EF Core

For production, you must replace the default `InMemory` services with persistent implementations. The ADK provides `EfCoreSessionService` to persist sessions, events, and state across restarts using Entity Framework Core.

```csharp
using GoogleAdk.Sessions.EfCore;
using Microsoft.EntityFrameworkCore;

var dbOptions = new DbContextOptionsBuilder<AdkSessionDbContext>()
    .UseSqlite("Data Source=adk_sessions.db")
    .Options;

var dbFactory = new PooledDbContextFactory<AdkSessionDbContext>(dbOptions);
var sessionService = new EfCoreSessionService(dbFactory);

// Provide the persistent service to your custom runner configuration
var runnerConfig = new RunnerConfig
{
    AppName = "production_app",
    Agent = myAgent,
    SessionService = sessionService,
    ArtifactService = new FileArtifactService("artifacts"), // Persist to disk
    MemoryService = new InMemoryMemoryService()
};

var runner = new Runner(runnerConfig);
```
