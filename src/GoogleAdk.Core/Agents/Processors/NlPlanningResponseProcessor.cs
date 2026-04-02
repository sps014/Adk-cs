using System.Runtime.CompilerServices;
using GoogleAdk.Core.Abstractions.Events;
using GoogleAdk.Core.Planning;

namespace GoogleAdk.Core.Agents.Processors;

/// <summary>
/// Applies planner response-side processing (e.g., stripping planning tags).
/// </summary>
public sealed class NlPlanningResponseProcessor : BaseLlmResponseProcessor
{
    public static readonly NlPlanningResponseProcessor Instance = new();

    public override async IAsyncEnumerable<Event> RunAsync(
        InvocationContext invocationContext,
        LlmResponse llmResponse)
    {
        if (invocationContext.Agent is not LlmAgent agent || agent.Planner == null)
            yield break;

        if (agent.Planner is BuiltInPlanner)
            yield break;

        if (llmResponse.Content?.Parts != null)
        {
            var callbackContext = new AgentContext(invocationContext);
            var updated = agent.Planner.ProcessPlanningResponse(callbackContext, llmResponse.Content.Parts);
            if (updated != null)
                llmResponse.Content.Parts = updated;

            if (HasActions(callbackContext.EventActions))
            {
                yield return Event.Create(e =>
                {
                    e.InvocationId = invocationContext.InvocationId;
                    e.Author = invocationContext.Agent.Name;
                    e.Branch = invocationContext.Branch;
                    e.Actions = callbackContext.EventActions;
                });
            }
        }

        await Task.CompletedTask;
    }

    private static bool HasActions(EventActions actions)
    {
        return actions.StateDelta.Count > 0
            || actions.ArtifactDelta.Count > 0
            || actions.RequestedAuthConfigs.Count > 0
            || actions.RequestedToolConfirmations.Count > 0
            || actions.TransferToAgent != null
            || actions.Escalate == true;
    }
}
