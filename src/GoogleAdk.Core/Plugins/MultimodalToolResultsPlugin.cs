using GoogleAdk.Core.Abstractions.Events;
using GoogleAdk.Core.Agents;

namespace GoogleAdk.Core.Plugins;

/// <summary>
/// Normalizes tool responses to support multimodal parts.
/// </summary>
public sealed class MultimodalToolResultsPlugin : BasePlugin
{
    public MultimodalToolResultsPlugin() : base(nameof(MultimodalToolResultsPlugin)) { }

    public override Task<Event?> OnEventCallbackAsync(InvocationContext invocationContext, Event evt)
    {
        return Task.FromResult<Event?>(null);
    }
}
