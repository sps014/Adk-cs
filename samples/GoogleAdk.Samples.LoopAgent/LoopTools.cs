using GoogleAdk.Core;
using GoogleAdk.Core.Abstractions.Events;
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Tools;

namespace GoogleAdk.Samples.LoopAgent;

/// <summary>
/// Tools for the loop agent sample.
/// </summary>
public static class LoopTools
{
    /// <summary>
    /// A tool that sets the escalate flag when the critic is satisfied,
    /// causing the LoopAgent to break its iteration cycle.
    /// </summary>
    public static readonly FunctionTool EscalateTool = new(
        name: "escalate",
        description: "Call this tool when the writing quality is satisfactory (score >= 8) to end the review loop.",
        execute: (args, ctx) =>
        {
            ctx.EventActions.Escalate = true;
            return Task.FromResult<object?>(new { status = "escalated", message = "Review loop completed." });
        },
        parameters: new Dictionary<string, object?>
        {
            ["type"] = "object",
            ["properties"] = new Dictionary<string, object?>(),
        });
}
