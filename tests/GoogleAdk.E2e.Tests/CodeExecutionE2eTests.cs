using GoogleAdk.Core.Abstractions.Events;
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Core.Abstractions.Sessions;
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.CodeExecutors;
using GoogleAdk.Core.Runner;

namespace GoogleAdk.E2e.Tests;

public class CodeExecutionE2eTests
{
    [Fact]
    public async Task BuiltInCodeExecution_AddsToolConfig()
    {
        var llm = new CapturingLlm("gemini-2.5-flash");
        var agent = new LlmAgent(new LlmAgentConfig
        {
            Name = "code-agent",
            Model = llm,
            CodeExecutor = new BuiltInCodeExecutor()
        });

        var runner = new InMemoryRunner("code-e2e", agent);
        var session = await runner.SessionService.CreateSessionAsync(new CreateSessionRequest
        {
            AppName = "code-e2e",
            UserId = "user-1",
        });

        var userMessage = new Content
        {
            Role = "user",
            Parts = new List<Part> { new Part { Text = "Hello" } }
        };

        await foreach (var _ in runner.RunAsync("user-1", session.Id, userMessage)) { }

        Assert.NotNull(llm.LastRequest?.Config?.Tools);
        Assert.Contains(llm.LastRequest!.Config!.Tools!, t => t.CodeExecution != null);
    }
}
