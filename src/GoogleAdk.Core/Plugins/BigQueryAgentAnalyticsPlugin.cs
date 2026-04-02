using GoogleAdk.Core.Abstractions.Events;
using GoogleAdk.Core.Agents;

namespace GoogleAdk.Core.Plugins;

/// <summary>
/// Placeholder for exporting agent analytics to BigQuery.
/// </summary>
public sealed class BigQueryAgentAnalyticsPlugin : BasePlugin
{
    public BigQueryAgentAnalyticsPlugin() : base(nameof(BigQueryAgentAnalyticsPlugin)) { }

    public override Task<Event?> OnEventCallbackAsync(InvocationContext invocationContext, Event evt)
    {
        return Task.FromResult<Event?>(null);
    }
}
