using GoogleAdk.Core.Abstractions.Events;
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Core.Abstractions.Sessions;
using GoogleAdk.Core.Abstractions.Tools;
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Agents.Processors;

namespace GoogleAdk.Core.Tests;

public class RequestConfirmationProcessorTests
{
    private sealed class EchoTool : Core.BaseTool
    {
        public EchoTool() : base("echo", "Echo tool") { }

        public override Task<object?> RunAsync(Dictionary<string, object?> args, AgentContext context)
        {
            return Task.FromResult<object?>(new Dictionary<string, object?> { ["ok"] = true });
        }
    }

    [Fact]
    public async Task RequestConfirmationProcessor_ResumesConfirmedToolCall()
    {
        var agent = new LlmAgent(new LlmAgentConfig
        {
            Name = "agent",
            Tools = [new EchoTool()]
        });

        var session = Session.Create("s1", "app", "user");
        var invocationContext = new InvocationContext
        {
            Agent = agent,
            Session = session,
            InvocationId = "inv-1",
        };

        var confirmationCall = new FunctionCall
        {
            Id = "confirm-1",
            Name = FunctionCallHandler.RequestConfirmationFunctionCallName,
            Args = new Dictionary<string, object?>
            {
                ["originalFunctionCall"] = new FunctionCall
                {
                    Id = "call-1",
                    Name = "echo",
                    Args = new Dictionary<string, object?> { ["text"] = "hi" }
                },
                ["toolConfirmation"] = new ToolConfirmation
                {
                    FunctionCallId = "call-1",
                    Accepted = true
                }
            }
        };

        session.Events.Add(Event.Create(e =>
        {
            e.Author = agent.Name;
            e.Content = new Content
            {
                Role = "model",
                Parts = [new Part { FunctionCall = confirmationCall }]
            };
        }));

        session.Events.Add(Event.Create(e =>
        {
            e.Author = "user";
            e.Content = new Content
            {
                Role = "user",
                Parts =
                [
                    new Part
                    {
                        FunctionResponse = new FunctionResponse
                        {
                            Id = "confirm-1",
                            Name = FunctionCallHandler.RequestConfirmationFunctionCallName,
                            Response = new Dictionary<string, object?>
                            {
                                ["toolConfirmation"] = new ToolConfirmation
                                {
                                    FunctionCallId = "call-1",
                                    Accepted = true
                                }
                            }
                        }
                    }
                ]
            };
        }));

        var processor = RequestConfirmationLlmRequestProcessor.Instance;
        var llmRequest = new LlmRequest();
        Event? responseEvent = null;

        await foreach (var evt in processor.RunAsync(invocationContext, llmRequest))
            responseEvent = evt;

        Assert.NotNull(responseEvent);
        var response = responseEvent!.GetFunctionResponses().FirstOrDefault();
        Assert.NotNull(response);
        Assert.Equal("call-1", response!.Id);
        Assert.Equal("echo", response.Name);
    }
}
