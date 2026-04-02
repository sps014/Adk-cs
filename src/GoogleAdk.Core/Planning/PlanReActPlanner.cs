using GoogleAdk.Core.Abstractions.Events;
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Core.Agents;

namespace GoogleAdk.Core.Planning;

/// <summary>
/// Planner that injects NL planning instructions and strips planning tags from responses.
/// </summary>
public sealed class PlanReActPlanner : IPlanner
{
    public const string PlanningTag = "/*PLANNING*/";
    public const string ReplanningTag = "/*REPLANNING*/";
    public const string ReasoningTag = "/*REASONING*/";
    public const string ActionTag = "/*ACTION*/";
    public const string FinalAnswerTag = "/*FINAL_ANSWER*/";

    public string? BuildPlanningInstruction(ReadonlyContext context, LlmRequest request)
    {
        return string.Join("\n",
        [
            "When appropriate, think through a short plan before acting.",
            $"Use the following tags in order: {PlanningTag} or {ReplanningTag}, {ReasoningTag}, {ActionTag}, {FinalAnswerTag}.",
            "Keep planning and reasoning concise and avoid revealing chain-of-thought in the final answer.",
        ]);
    }

    public List<Part>? ProcessPlanningResponse(AgentContext context, List<Part> responseParts)
    {
        if (responseParts.Count == 0)
            return null;

        var processed = new List<Part>();
        foreach (var part in responseParts)
        {
            if (part.FunctionCall != null)
            {
                processed.Add(part);
                continue;
            }

            if (part.Text == null)
            {
                processed.Add(part);
                continue;
            }

            var text = part.Text;
            var finalIndex = text.IndexOf(FinalAnswerTag, StringComparison.Ordinal);
            if (finalIndex >= 0)
            {
                text = text[(finalIndex + FinalAnswerTag.Length)..];
            }

            text = text.Replace(PlanningTag, string.Empty, StringComparison.Ordinal)
                .Replace(ReplanningTag, string.Empty, StringComparison.Ordinal)
                .Replace(ReasoningTag, string.Empty, StringComparison.Ordinal)
                .Replace(ActionTag, string.Empty, StringComparison.Ordinal);

            if (!string.IsNullOrWhiteSpace(text))
            {
                processed.Add(new Part { Text = text.Trim() });
            }
        }

        return processed;
    }
}
