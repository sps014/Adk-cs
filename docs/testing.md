# Testing

The ADK uses standard xUnit for unit and end-to-end testing. The testing suite focuses on verifying the execution of the agent pipeline, tool invocation reliability, processor order operations, and integration via the A2A protocol.

## Running Tests

To run the entire suite:

```bash
cd Adk/GoogleAdk
dotnet test
```

## Structure

- **`GoogleAdk.Core.Tests`**: Contains unit tests for individual processors, memory services, telemetry tracking, and core logic primitives.
- **`GoogleAdk.E2e.Tests`**: Contains tests that simulate full multi-turn conversational flows, A2A REST HTTP endpoints, and complete orchestration pipelines like Sequential and Parallel agents.

## Testing with InMemory Services

Because the ADK abstracts persistent storage into `SessionService`, `ArtifactService`, and `MemoryService`, writing tests for your complex agent orchestrations is extremely easy without requiring mock databases.

You can rapidly test your agent behavior using the `InMemoryRunner`.

```csharp
[Fact]
public async Task CustomAgent_ExecutesTool_AndReturnsSummary()
{
    var agent = new LlmAgent(new LlmAgentConfig { ... });
    
    // Bootstraps isolated in-memory stores perfect for testing 
    var runner = new InMemoryRunner("test-app", agent);
    var session = await runner.SessionService.CreateSessionAsync(new CreateSessionRequest { AppName = "test-app", UserId = "user-1" });
    
    var userMessage = new Content { Role = "user", Parts = [new Part { Text = "Test query" }] };
    
    var responseReceived = false;
    await foreach (var evt in runner.RunAsync("user-1", session.Id, userMessage))
    {
        if (evt.IsFinalResponse() && evt.Content?.Parts != null)
        {
            responseReceived = true;
        }
    }
    
    Assert.True(responseReceived);
}
```