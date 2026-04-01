using GoogleAdk.Core.Abstractions.Events;
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Core.Abstractions.Sessions;
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Examples;
using GoogleAdk.Core.Runner;

namespace GoogleAdk.E2e.Tests;

public class ExamplesE2eTests
{
    private sealed class CapturingLlm : BaseLlm
    {
        public LlmRequest? LastRequest { get; private set; }

        public CapturingLlm(string model) : base(model) { }

        public override async IAsyncEnumerable<LlmResponse> GenerateContentAsync(
            LlmRequest llmRequest,
            bool stream = false,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            LastRequest = llmRequest;
            yield return new LlmResponse
            {
                Content = new Content
                {
                    Role = "model",
                    Parts = new List<Part> { new Part { Text = "ok" } }
                }
            };
            await Task.CompletedTask;
        }

        public override Task<BaseLlmConnection> ConnectAsync(LlmRequest llmRequest)
        {
            throw new NotSupportedException();
        }
    }

    [Fact]
    public async Task ExampleInjection_AppendsSystemInstruction()
    {
        var llm = new CapturingLlm("gemini-2.5-flash");
        var agent = new LlmAgent(new LlmAgentConfig
        {
            Name = "example-agent",
            Model = llm,
            Instruction = "base",
            Examples =
            [
                new Example
                {
                    Input = new Content
                    {
                        Role = "user",
                        Parts = new List<Part> { new Part { Text = "ping" } }
                    },
                    Output =
                    [
                        new Content
                        {
                            Role = "model",
                            Parts = new List<Part> { new Part { Text = "pong" } }
                        }
                    ]
                }
            ]
        });

        var runner = new InMemoryRunner("examples-e2e", agent);
        var session = await runner.SessionService.CreateSessionAsync(new CreateSessionRequest
        {
            AppName = "examples-e2e",
            UserId = "user-1",
        });

        var userMessage = new Content
        {
            Role = "user",
            Parts = new List<Part> { new Part { Text = "Hello" } }
        };

        await foreach (var _ in runner.RunAsync("user-1", session.Id, userMessage)) { }

        Assert.NotNull(llm.LastRequest?.Config?.SystemInstruction);
        Assert.Contains("<EXAMPLES>", llm.LastRequest!.Config!.SystemInstruction);
    }
}
