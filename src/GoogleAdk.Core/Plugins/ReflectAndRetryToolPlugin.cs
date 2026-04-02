using GoogleAdk.Core.Agents;

namespace GoogleAdk.Core.Plugins;

/// <summary>
/// Placeholder plugin for reflecting on tool errors and retrying.
/// </summary>
public sealed class ReflectAndRetryToolPlugin : BasePlugin
{
    public ReflectAndRetryToolPlugin() : base(nameof(ReflectAndRetryToolPlugin)) { }

    public override Task<Dictionary<string, object?>?> AfterToolCallbackAsync(
        BaseTool tool,
        Dictionary<string, object?> toolArgs,
        AgentContext toolContext,
        Dictionary<string, object?> result)
    {
        return Task.FromResult<Dictionary<string, object?>?>(null);
    }
}
