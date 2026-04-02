using GoogleAdk.Core.Abstractions.Events;

namespace GoogleAdk.Core.Agents.Processors;

/// <summary>
/// Placeholder processor for model context cache configuration.
/// </summary>
public sealed class ContextCacheRequestProcessor : BaseLlmRequestProcessor
{
    public static readonly ContextCacheRequestProcessor Instance = new();

    public override async IAsyncEnumerable<Event> RunAsync(
        InvocationContext invocationContext,
        LlmRequest llmRequest)
    {
        if (invocationContext.Agent is not LlmAgent agent || agent.ContextCacheConfig == null)
            yield break;

        await Task.CompletedTask;
    }
}
