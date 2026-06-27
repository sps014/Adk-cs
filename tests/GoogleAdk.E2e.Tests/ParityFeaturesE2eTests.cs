using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using GoogleAdk.ApiServer;
using GoogleAdk.Core.A2a;
using GoogleAdk.Core.Abstractions.Events;
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Core.Abstractions.Sessions;
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.CodeExecutors;
using GoogleAdk.Core.Examples;
using GoogleAdk.Core.Planning;
using GoogleAdk.Core.Runner;
using GoogleAdk.Core.Sessions;
using GoogleAdk.Core.Tools;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using A2aTaskStatus = GoogleAdk.Core.A2a.TaskStatus;
using Task = System.Threading.Tasks.Task;

namespace GoogleAdk.E2e.Tests;

/// <summary>
/// Deterministic (mock-based) coverage for the parity/stabilization features added in this
/// effort: the A2A task-result aggregator, agent-card builder parity, JSON-RPC error handling,
/// well-known card path interop, model callback lists, and the streaming output_key guard.
/// These run without any network/LLM credentials.
/// </summary>
public class ParityFeaturesE2eTests
{
    // ---------- TaskResultAggregator ----------

    [Fact]
    public void TaskResultAggregator_FailedBeatsEverything()
    {
        var agg = new TaskResultAggregator();
        agg.Process(StatusEvent(TaskState.Working));
        agg.Process(StatusEvent(TaskState.InputRequired));
        agg.Process(StatusEvent(TaskState.Failed));
        agg.Process(StatusEvent(TaskState.Working)); // must not downgrade

        Assert.Equal(TaskState.Failed, agg.TaskState);
    }

    [Fact]
    public void TaskResultAggregator_AuthRequiredBeatsInputRequiredAndWorking()
    {
        var agg = new TaskResultAggregator();
        agg.Process(StatusEvent(TaskState.Working));
        agg.Process(StatusEvent(TaskState.InputRequired));
        agg.Process(StatusEvent(TaskState.AuthRequired));
        agg.Process(StatusEvent(TaskState.Working));

        Assert.Equal(TaskState.AuthRequired, agg.TaskState);
    }

    [Fact]
    public void TaskResultAggregator_InputRequiredBeatsWorking()
    {
        var agg = new TaskResultAggregator();
        agg.Process(StatusEvent(TaskState.Working));
        agg.Process(StatusEvent(TaskState.InputRequired));
        agg.Process(StatusEvent(TaskState.Working));

        Assert.Equal(TaskState.InputRequired, agg.TaskState);
    }

    [Fact]
    public void TaskResultAggregator_WorkingWhenNothingHigher()
    {
        var agg = new TaskResultAggregator();
        agg.Process(StatusEvent(TaskState.Working));
        Assert.Equal(TaskState.Working, agg.TaskState);
    }

    private static TaskStatusUpdateEvent StatusEvent(string state) => new()
    {
        TaskId = "t1",
        ContextId = "c1",
        Status = new A2aTaskStatus { State = state },
    };

    // ---------- Agent card builder parity ----------

    [Fact]
    public async Task AgentCard_IncludesPlannerCodeExecutorAndExampleSkills()
    {
        var agent = new LlmAgent(new LlmAgentConfig
        {
            Name = "rich-agent",
            Description = "An agent with planner, code executor, and examples.",
            Model = MockLlm.FromResponses(new LlmResponse
            {
                Content = new Content { Role = "model", Parts = [new Part { Text = "hi" }] },
            }),
            Planner = new PlanReActPlanner(),
            CodeExecutor = new BuiltInCodeExecutor(),
            Examples =
            [
                new Example
                {
                    Input = new Content { Role = "user", Parts = [new Part { Text = "What is 2+2?" }] },
                    Output = [new Content { Role = "model", Parts = [new Part { Text = "4" }] }],
                },
            ],
        });

        var transports = new[] { new AgentInterface { Url = "http://localhost/jsonrpc", Transport = "JSONRPC" } };
        var card = await AgentCardBuilder.GetA2AAgentCardAsync(agent, transports);

        Assert.Contains(card.Skills, s => s.Name == "planning");
        Assert.Contains(card.Skills, s => s.Name == "code-execution");
        var modelSkill = Assert.Single(card.Skills, s => s.Name == "model");
        Assert.NotNull(modelSkill.Examples);
        Assert.Contains("What is 2+2?", modelSkill.Examples!);
    }

    [Fact]
    public void AgentCard_ProviderAndDocFields_RoundTripThroughJson()
    {
        var card = new AgentCard
        {
            Name = "card",
            Provider = new AgentProvider { Organization = "Acme", Url = "https://acme.example" },
            DocumentationUrl = "https://docs.example",
            SecuritySchemes = new Dictionary<string, object?> { ["apiKey"] = "header" },
        };

        var json = JsonSerializer.Serialize(card);
        var round = JsonSerializer.Deserialize<AgentCard>(json)!;

        Assert.Equal("Acme", round.Provider?.Organization);
        Assert.Equal("https://docs.example", round.DocumentationUrl);
        Assert.NotNull(round.SecuritySchemes);
    }

    // ---------- A2A JSON-RPC error + card path interop ----------

    [Fact]
    public async Task A2aJsonRpc_UnknownMethod_ReturnsMethodNotFound()
    {
        await using var app = await StartA2aServerAsync("a2a-unknown-method");
        var client = app.GetTestClient();

        var response = await client.PostAsJsonAsync(
            "a2a/a2a-unknown-method/jsonrpc",
            new { jsonrpc = "2.0", id = "1", method = "tasks/cancel", @params = new { } });

        response.EnsureSuccessStatusCode();
        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(doc.RootElement.TryGetProperty("error", out var error));
        Assert.Equal(-32601, error.GetProperty("code").GetInt32());

        await app.StopAsync();
    }

    [Fact]
    public async Task A2aCard_LegacyWellKnownPath_IsServed()
    {
        await using var app = await StartA2aServerAsync("a2a-legacy-card");
        var client = app.GetTestClient();

        var v2 = await client.GetAsync("a2a/a2a-legacy-card/.well-known/agent-card.json");
        var legacy = await client.GetAsync("a2a/a2a-legacy-card/.well-known/agent.json");

        Assert.Equal(HttpStatusCode.OK, v2.StatusCode);
        Assert.Equal(HttpStatusCode.OK, legacy.StatusCode);

        var legacyCard = JsonSerializer.Deserialize<AgentCard>(
            await legacy.Content.ReadAsStringAsync(),
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.Equal("a2a-legacy-card", legacyCard?.Name);

        await app.StopAsync();
    }

    [Fact]
    public async Task ResolveAgentCard_FromBaseUrl_ProbesWellKnownPaths()
    {
        await using var app = await StartA2aServerAsync("a2a-resolve-card");
        var client = app.GetTestClient();

        var baseUrl = new Uri(client.BaseAddress!, "a2a/a2a-resolve-card").ToString();
        var card = await AgentCardBuilder.ResolveAgentCardAsync(baseUrl, client);

        Assert.Equal("a2a-resolve-card", card.Name);
        await app.StopAsync();
    }

    private static async Task<WebApplication> StartA2aServerAsync(string appName)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        var loader = new AgentLoader(Path.GetTempPath());
        loader.Register(appName, new LlmAgent(new LlmAgentConfig
        {
            Name = appName,
            Description = "Deterministic test agent.",
            Model = MockLlm.FromResponses(new LlmResponse
            {
                Content = new Content { Role = "model", Parts = [new Part { Text = "ok" }] },
            }),
        }));
        builder.Services.AddSingleton(loader);
        builder.Services.AddSingleton<BaseSessionService, InMemorySessionService>();
        builder.Services.AddSingleton<RunnerManager>();

        var app = builder.Build();
        app.MapA2aApi();
        await app.StartAsync();
        return app;
    }

    // ---------- Model callback lists ----------

    [Fact]
    public async Task BeforeModelCallbacks_RunInOrder()
    {
        var order = new List<string>();
        var agent = new LlmAgent(new LlmAgentConfig
        {
            Name = "cb-agent",
            Model = MockLlm.FromResponses(new LlmResponse
            {
                Content = new Content { Role = "model", Parts = [new Part { Text = "done" }] },
            }),
            BeforeModelCallbacks =
            [
                (_, _) => { order.Add("first"); return Task.FromResult<LlmResponse?>(null); },
                (_, _) => { order.Add("second"); return Task.FromResult<LlmResponse?>(null); },
            ],
        });

        var (runner, sessionId) = await SetupAsync(agent, "cb-test");
        await foreach (var _ in runner.RunAsync("user-1", sessionId, UserMessage("hi"))) { }

        Assert.Equal(new[] { "first", "second" }, order);
    }

    [Fact]
    public async Task BeforeModelCallback_ShortCircuits_WhenResponseReturned()
    {
        var modelCalled = false;
        var capturing = new CountingMockLlm(() => modelCalled = true);
        var agent = new LlmAgent(new LlmAgentConfig
        {
            Name = "cb-shortcircuit",
            Model = capturing,
            BeforeModelCallbacks =
            [
                (_, _) => Task.FromResult<LlmResponse?>(new LlmResponse
                {
                    Content = new Content { Role = "model", Parts = [new Part { Text = "from-callback" }] },
                }),
            ],
        });

        var (runner, sessionId) = await SetupAsync(agent, "cb-sc-test");
        var events = new List<Event>();
        await foreach (var e in runner.RunAsync("user-1", sessionId, UserMessage("hi"))) events.Add(e);

        Assert.False(modelCalled, "Model must be skipped when a before-model callback returns a response");
        Assert.Contains(events, e => e.Content?.Parts?.Any(p => p.Text == "from-callback") == true);
    }

    // ---------- Streaming output_key guard ----------

    [Fact]
    public async Task OutputKey_NotOverwritten_ByToolOnlyEvents()
    {
        var weatherTool = new FunctionTool(
            "get_weather", "Gets weather",
            (_, _) => Task.FromResult<object?>(new Dictionary<string, object?> { ["result"] = "Sunny" }),
            new Dictionary<string, object?>
            {
                ["type"] = "object",
                ["properties"] = new Dictionary<string, object?>
                {
                    ["city"] = new Dictionary<string, object?> { ["type"] = "string" },
                },
            });

        var llm = MockLlm.FromGroups(
            new List<LlmResponse>
            {
                new()
                {
                    Content = new Content
                    {
                        Role = "model",
                        Parts = [new Part { FunctionCall = new FunctionCall { Name = "get_weather", Args = new() { ["city"] = "NY" } } }],
                    },
                },
            },
            new List<LlmResponse>
            {
                new() { Content = new Content { Role = "model", Parts = [new Part { Text = "It is sunny in NY." }] } },
            });

        var agent = new LlmAgent(new LlmAgentConfig
        {
            Name = "output-key-agent",
            Model = llm,
            Tools = [weatherTool],
            OutputKey = "summary",
        });

        var (runner, sessionId) = await SetupAsync(agent, "output-key-test");
        await foreach (var _ in runner.RunAsync("user-1", sessionId, UserMessage("Weather in NY?"))) { }

        var session = await runner.SessionService.GetSessionAsync(new GetSessionRequest
        {
            AppName = "output-key-test",
            UserId = "user-1",
            SessionId = sessionId,
        });

        Assert.NotNull(session);
        Assert.True(session!.State.TryGetValue("summary", out var value));
        var summary = value?.ToString();
        Assert.False(string.IsNullOrWhiteSpace(summary), "output_key must not be empty/whitespace");
        Assert.Contains("It is sunny in NY.", summary!);
    }

    // ---------- helpers ----------

    private static Content UserMessage(string text) => new()
    {
        Role = "user",
        Parts = [new Part { Text = text }],
    };

    private static async Task<(InMemoryRunner runner, string sessionId)> SetupAsync(BaseAgent agent, string appName)
    {
        var runner = new InMemoryRunner(appName, agent);
        var session = await runner.SessionService.CreateSessionAsync(new CreateSessionRequest
        {
            AppName = appName,
            UserId = "user-1",
        });
        return (runner, session.Id);
    }

    private sealed class CountingMockLlm : BaseLlm
    {
        private readonly Action _onCall;
        public CountingMockLlm(Action onCall) : base("counting-mock") => _onCall = onCall;

        public override async IAsyncEnumerable<LlmResponse> GenerateContentAsync(
            LlmRequest llmRequest,
            bool stream = false,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            _onCall();
            yield return new LlmResponse
            {
                Content = new Content { Role = "model", Parts = [new Part { Text = "from-model" }] },
            };
            await Task.CompletedTask;
        }

        public override Task<BaseLlmConnection> ConnectAsync(LlmRequest llmRequest)
            => throw new NotSupportedException();
    }
}
