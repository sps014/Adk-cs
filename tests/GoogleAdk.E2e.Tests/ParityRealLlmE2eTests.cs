using GoogleAdk.Core.Abstractions.Events;
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Core.Abstractions.Sessions;
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.CodeExecutors;
using GoogleAdk.Core.Planning;
using GoogleAdk.Core.Runner;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace GoogleAdk.E2e.Tests;

/// <summary>
/// Real-Gemini integration coverage for parity features (code execution, planner, multi-turn
/// session context, callback lists). Each test skips automatically when no API key is configured.
/// </summary>
public class ParityRealLlmE2eTests
{
    [RealLlmFact]
    public async Task RealLlm_BuiltInCodeExecutor_RunsCodeAndComputesResult()
    {
        var agent = new LlmAgent(new LlmAgentConfig
        {
            Name = "coder",
            Model = "gemini-2.5-flash",
            Instruction = "You can run Python. When asked to compute, write and execute code, then state the final number.",
            CodeExecutor = new BuiltInCodeExecutor(),
        });

        var runner = new InMemoryRunner("code-exec-test", agent);
        var session = await runner.SessionService.CreateSessionAsync(new CreateSessionRequest
        {
            AppName = "code-exec-test",
            UserId = "user-1",
        });

        var allParts = new List<Part>();
        await foreach (var evt in runner.RunAsync("user-1", session.Id,
            UserMessage("Using Python code execution, compute the sum of all integers from 1 to 100 and report the number.")))
        {
            allParts.AddRange(evt.Content?.Parts ?? new List<Part>());
        }

        // The built-in code-execution path should produce executable code and/or a
        // code-execution result; the answer (5050) may surface in any of those parts.
        var ranCode = allParts.Any(p => p.ExecutableCode != null || p.CodeExecutionResult != null);
        var combined = string.Join(" ", allParts.Select(p =>
            p.Text ?? p.CodeExecutionResult?.Output ?? p.ExecutableCode?.Code ?? ""));

        Assert.True(ranCode || combined.Contains("5050"),
            $"Expected code execution or the computed answer. Got: {combined}");
    }

    [RealLlmFact]
    public async Task RealLlm_Planner_ProducesCorrectAnswer()
    {
        var agent = new LlmAgent(new LlmAgentConfig
        {
            Name = "planner-agent",
            Model = "gemini-2.5-flash",
            Instruction = "Think step by step, then give the final numeric answer.",
            Planner = new PlanReActPlanner(),
        });

        var text = await RunToTextAsync(agent, "planner-real-test",
            "A train travels 60 km in 1.5 hours. What is its average speed in km/h? Give just the number at the end.");

        Assert.Contains("40", text);
    }

    [RealLlmFact]
    public async Task RealLlm_MultiTurn_MaintainsSessionContext()
    {
        var agent = new LlmAgent(new LlmAgentConfig
        {
            Name = "memory-agent",
            Model = "gemini-2.5-flash",
            Instruction = "You are a helpful assistant with memory of the conversation.",
        });

        var runner = new InMemoryRunner("multiturn-test", agent);
        var session = await runner.SessionService.CreateSessionAsync(new CreateSessionRequest
        {
            AppName = "multiturn-test",
            UserId = "user-1",
        });

        await DrainAsync(runner, session.Id, "My favorite color is teal. Please remember it.");
        var secondTurn = await CollectTextAsync(runner, session.Id, "What is my favorite color? Answer with one word.");

        Assert.Contains("teal", secondTurn.ToLowerInvariant());
    }

    [RealLlmFact]
    public async Task RealLlm_BeforeModelCallbackList_BothObserveRequest()
    {
        var observed = new List<string>();
        var agent = new LlmAgent(new LlmAgentConfig
        {
            Name = "cb-real-agent",
            Model = "gemini-2.5-flash",
            Instruction = "Reply with one short sentence.",
            BeforeModelCallbacks =
            [
                (_, _) => { observed.Add("a"); return Task.FromResult<LlmResponse?>(null); },
                (_, _) => { observed.Add("b"); return Task.FromResult<LlmResponse?>(null); },
            ],
        });

        var text = await RunToTextAsync(agent, "cb-real-test", "Say hello.");

        Assert.Equal(new[] { "a", "b" }, observed);
        Assert.False(string.IsNullOrWhiteSpace(text));
    }

    // ---------- helpers ----------

    private static async Task<string> RunToTextAsync(BaseAgent agent, string appName, string prompt)
    {
        var runner = new InMemoryRunner(appName, agent);
        var session = await runner.SessionService.CreateSessionAsync(new CreateSessionRequest
        {
            AppName = appName,
            UserId = "user-1",
        });
        return await CollectTextAsync(runner, session.Id, prompt);
    }

    private static async Task<string> CollectTextAsync(Runner runner, string sessionId, string prompt)
    {
        var parts = new List<string>();
        await foreach (var evt in runner.RunAsync("user-1", sessionId, UserMessage(prompt)))
        {
            foreach (var part in evt.Content?.Parts ?? new List<Part>())
                if (part.Text != null) parts.Add(part.Text);
        }
        return string.Join(" ", parts);
    }

    private static async Task DrainAsync(Runner runner, string sessionId, string prompt)
    {
        await foreach (var _ in runner.RunAsync("user-1", sessionId, UserMessage(prompt))) { }
    }

    private static Content UserMessage(string text) => new()
    {
        Role = "user",
        Parts = [new Part { Text = text }],
    };
}
