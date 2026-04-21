# Custom Runner

While the ADK provides `AdkServer`, `ConsoleRunner`, and `InMemoryRunner` for convenience, production applications often require a custom execution environment to integrate deeply with existing architectures (like Discord bots, WPF apps, background worker services, or custom APIs).

You can achieve this by instantiating the core `Runner` class directly.

## Building a Custom Runner

To build a custom runner, you need to configure the three core storage services: Sessions, Artifacts, and Memory.

```csharp
using GoogleAdk.Core.Runner;
using GoogleAdk.Sessions.EfCore;
using GoogleAdk.Core.Artifacts;
using GoogleAdk.Core.Memory;

// 1. Initialize persistent services
var sessionService = new EfCoreSessionService(dbFactory);
var artifactService = new FileArtifactService("./adk_artifacts");
var memoryService = new InMemoryMemoryService(); // Replace with persistent memory if needed

// 2. Configure the Runner
var runnerConfig = new RunnerConfig
{
    AppName = "my_custom_bot",
    Agent = myAgent,
    SessionService = sessionService,
    ArtifactService = artifactService,
    MemoryService = memoryService,
    RunConfig = new RunConfig
    {
        MaxLlmCalls = 15,
        SaveInputBlobsAsArtifacts = true
    }
};

var runner = new Runner(runnerConfig);
```

## Invoking the Runner

Once configured, you must explicitly manage the session lifecycle before invoking `RunAsync`.

```csharp
// 3. Create or retrieve a session for the user
var session = await runner.SessionService.CreateSessionAsync(new CreateSessionRequest
{
    AppName = "my_custom_bot",
    UserId = "discord-user-123"
});

// 4. Construct the user payload
var userMessage = new Content
{
    Role = "user",
    Parts = [new Part { Text = "Hello, what can you do?" }]
};

// 5. Run the invocation stream
await foreach (var evt in runner.RunAsync("discord-user-123", session.Id, userMessage))
{
    // Process the event stream depending on your application needs
    if (evt.Partial == true && evt.Content?.Parts != null)
    {
        // Handle streaming text chunks
    }
    
    if (evt.IsFinalResponse() && evt.Content?.Parts != null)
    {
        // Handle the final completed message
        var text = evt.StringifyContent();
        await SendMessageToDiscord(text);
    }
    
    var calls = evt.GetFunctionCalls();
    if (calls.Count > 0)
    {
        // Optionally log or track tool calls
    }
}
```

## Abstracting with a Wrapper

If your application has a specific domain (like a Discord bot or a specific UI framework), you can wrap this logic in your own static or instance runner class, similar to how the `ConsoleRunner` is implemented in the ADK Core. This allows you to encapsulate the `RunAsync` loop and translate ADK `Event` objects directly into your domain's UI or messaging primitives.
