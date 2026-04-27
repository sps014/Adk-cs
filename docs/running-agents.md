# Running Agents

Agents must execute within a `Runner` environment that manages session state, artifacts, memory, and telemetry.

## Available Runners

- **[ADK Web](adk-web.md)**: Interactive Blazor dashboard for visual debugging.
- **[ADK Server A2A](adk-server-a2a.md)**: HTTP APIs for synchronous, streaming, and Agent-to-Agent execution.
- **[Console Runner](console-runner.md)**: Interactive terminal experience using `Spectre.Console`.
- **[Custom Runner](custom-runner.md)**: Core `Runner` class for deep integration into background services.

## Core `Runner` Invocation

Execution requires a `Runner`, a `SessionId` (tied to a user and app), and input `Content`.

```csharp
var runner = new InMemoryRunner("cli-app", myAgent);

// Fetch or create a session
var session = await runner.SessionService.CreateSessionAsync(new CreateSessionRequest
{
    AppName = "cli-app", UserId = "dev-user"
});

// Run and stream events
await foreach (var evt in runner.RunAsync("dev-user", session.Id, userMessage))
{
    // Handle output
}
```

## Run Configurations

Control execution behavior, limits, and multimodality using `RunConfig`:

```csharp
var runConfig = new RunConfig
{
    MaxLlmCalls = 10,
    PauseOnToolCalls = true,
    ResponseModalities = ["AUDIO"],
    SaveInputBlobsAsArtifacts = true
};
```

## Persistent Sessions

Replace default `InMemory` services with `EfCoreSessionService` for production databases (e.g., SQLite, Postgres).

```csharp
var dbOptions = new DbContextOptionsBuilder<AdkSessionDbContext>()
    .UseSqlite("Data Source=adk_sessions.db")
    .Options;

var runnerConfig = new RunnerConfig
{
    AppName = "production_app",
    Agent = myAgent,
    SessionService = new EfCoreSessionService(new PooledDbContextFactory<AdkSessionDbContext>(dbOptions)),
    ArtifactService = new FileArtifactService("artifacts") // Persist to disk
};

var runner = new Runner(runnerConfig);
```