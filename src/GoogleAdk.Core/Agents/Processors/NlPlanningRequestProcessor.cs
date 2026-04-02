using GoogleAdk.Core.Abstractions.Events;
using GoogleAdk.Core.Planning;

namespace GoogleAdk.Core.Agents.Processors;

/// <summary>
/// Adds planner instructions and applies planner request-side behavior.
/// </summary>
public sealed class NlPlanningRequestProcessor : BaseLlmRequestProcessor
{
    public static readonly NlPlanningRequestProcessor Instance = new();

    public override async IAsyncEnumerable<Event> RunAsync(
        InvocationContext invocationContext,
        LlmRequest llmRequest)
    {
        if (invocationContext.Agent is not LlmAgent agent || agent.Planner == null)
            yield break;

        var readonlyContext = new ReadonlyContext(invocationContext);
        var instruction = agent.Planner.BuildPlanningInstruction(readonlyContext, llmRequest);
        if (!string.IsNullOrWhiteSpace(instruction))
            llmRequest.AppendInstructions(instruction);

        if (agent.Planner is PlanReActPlanner)
        {
            foreach (var content in llmRequest.Contents)
            {
                if (content.Parts == null) continue;
                foreach (var part in content.Parts)
                    part.Thought = null;
            }
        }

        await Task.CompletedTask;
    }
}
