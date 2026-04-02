using GoogleAdk.Core.Abstractions.Events;
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Examples;

namespace GoogleAdk.Core.Tools;

/// <summary>
/// Tool-like hook that injects few-shot examples into the request.
/// </summary>
public sealed class ExampleTool : BaseTool
{
    private readonly List<Example> _examples;
    private readonly BaseExampleProvider? _provider;

    public ExampleTool(List<Example> examples, BaseExampleProvider? provider = null)
        : base("example_tool", "Injects few-shot examples into the prompt.")
    {
        _examples = examples;
        _provider = provider;
    }

    public override FunctionDeclaration? GetDeclaration() => null;

    public override Task<object?> RunAsync(Dictionary<string, object?> args, AgentContext context)
        => Task.FromResult<object?>(null);

    public override Task ProcessLlmRequestAsync(AgentContext context, LlmRequest llmRequest)
    {
        var query = context.InvocationContext.UserContent?.Parts?.FirstOrDefault()?.Text ?? string.Empty;
        var examplesText = _provider != null
            ? ExampleUtil.BuildExampleSystemInstruction(_provider, query, llmRequest.Model)
            : ExampleUtil.BuildExampleSystemInstruction(_examples, llmRequest.Model);

        if (!string.IsNullOrWhiteSpace(examplesText))
            llmRequest.AppendInstructions(examplesText);

        return Task.CompletedTask;
    }
}
