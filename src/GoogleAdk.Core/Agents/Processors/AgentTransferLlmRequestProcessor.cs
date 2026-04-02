using System.Runtime.CompilerServices;
using GoogleAdk.Core.Abstractions.Events;
using GoogleAdk.Core.Tools;

namespace GoogleAdk.Core.Agents.Processors;

/// <summary>
/// Adds a transfer_to_agent tool and instructions about available agents to transfer to.
/// Handles parent, peer, and sub-agent transfers based on agent configuration.
/// </summary>
public class AgentTransferLlmRequestProcessor : BaseLlmRequestProcessor
{
    public static readonly AgentTransferLlmRequestProcessor Instance = new();

    public override async IAsyncEnumerable<Event> RunAsync(
        InvocationContext invocationContext,
        LlmRequest llmRequest)
    {
        await Task.CompletedTask;

        if (invocationContext.Agent is not LlmAgent agent)
            yield break;

        var transferTargets = GetTransferTargets(agent);
        if (transferTargets.Count == 0)
            yield break;

        // Add transfer instructions
        llmRequest.AppendInstructions(BuildTargetAgentsInstructions(agent, transferTargets));

        var transferTool = new TransferToAgentTool(transferTargets.Select(t => t.Name).ToList());
        llmRequest.AppendTools(new[] { transferTool });
    }

    private static string BuildTargetAgentsInstructions(LlmAgent agent, List<BaseAgent> targets)
    {
        var agentInfos = string.Join("\n", targets.Select(t =>
            $"Agent name: {t.Name}\nAgent description: {t.Description ?? "(no description)"}\n"));

        var instructions = $@"
You have a list of other agents to transfer to:

{agentInfos}

If you are the best to answer the question according to your description, you
can answer it.

If another agent is better for answering the question according to its
description, call `transfer_to_agent` function to transfer the
question to that agent. When transferring, do not generate any text other than
the function call.
";

        if (agent.ParentAgent != null && !agent.DisallowTransferToParent)
        {
            instructions += $@"
Your parent agent is {agent.ParentAgent.Name}. If neither the other agents nor
you are best for answering the question according to the descriptions, transfer
to your parent agent.
";
        }

        return instructions;
    }

    private static List<BaseAgent> GetTransferTargets(LlmAgent agent)
    {
        var targets = new List<BaseAgent>();
        targets.AddRange(agent.SubAgents);

        if (agent.ParentAgent is not LlmAgent)
            return targets;

        if (!agent.DisallowTransferToParent)
            targets.Add(agent.ParentAgent);

        if (!agent.DisallowTransferToPeers)
        {
            targets.AddRange(
                agent.ParentAgent.SubAgents.Where(peer => peer.Name != agent.Name));
        }

        return targets;
    }
}
