using GoogleAdk.Core.Abstractions.Events;
using GoogleAdk.Core.Agents;

namespace GoogleAdk.Core.Plugins;

/// <summary>
/// Appends a global instruction to each model request.
/// </summary>
public sealed class GlobalInstructionPlugin : BasePlugin
{
    private readonly string _instruction;

    public GlobalInstructionPlugin(string instruction) : base(nameof(GlobalInstructionPlugin))
    {
        _instruction = instruction;
    }

    public override Task<LlmResponse?> BeforeModelCallbackAsync(
        AgentContext callbackContext,
        LlmRequest llmRequest)
    {
        if (!string.IsNullOrWhiteSpace(_instruction))
            llmRequest.AppendInstructions(_instruction);
        return Task.FromResult<LlmResponse?>(null);
    }
}
