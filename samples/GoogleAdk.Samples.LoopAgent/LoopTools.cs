using GoogleAdk.Core;
using GoogleAdk.Core.Abstractions.Events;
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Abstractions.Tools;

namespace GoogleAdk.Samples.LoopAgent;

/// <summary>
/// Tools for the loop agent sample.
/// </summary>
public static partial class LoopTools
{
    /// <summary>
    /// A tool that sets the escalate flag when the critic is satisfied,
    /// causing the LoopAgent to break its iteration cycle.
    /// </summary>
    /// <param name="context">Agent context for setting escalate.</param>
    [FunctionTool(Name = "escalate")]
    public static object? Escalate(AgentContext context)
    {
        context.EventActions.Escalate = true;
        return new { status = "escalated", message = "Review loop completed." };
    }
}
