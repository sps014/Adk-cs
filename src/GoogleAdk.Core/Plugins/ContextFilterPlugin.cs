using GoogleAdk.Core.Abstractions.Events;
using GoogleAdk.Core.Agents;

namespace GoogleAdk.Core.Plugins;

/// <summary>
/// Filters context to the last N contents to limit prompt size.
/// </summary>
public sealed class ContextFilterPlugin : BasePlugin
{
    private readonly int _maxContents;

    public ContextFilterPlugin(int maxContents = 50) : base(nameof(ContextFilterPlugin))
    {
        _maxContents = maxContents;
    }

    public override Task<LlmResponse?> BeforeModelCallbackAsync(
        AgentContext callbackContext,
        LlmRequest llmRequest)
    {
        if (llmRequest.Contents.Count > _maxContents)
        {
            llmRequest.Contents = llmRequest.Contents
                .Skip(Math.Max(0, llmRequest.Contents.Count - _maxContents))
                .ToList();
        }
        return Task.FromResult<LlmResponse?>(null);
    }
}
