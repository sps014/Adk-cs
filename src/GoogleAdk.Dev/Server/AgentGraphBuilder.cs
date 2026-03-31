// Copyright 2025 Google LLC
// SPDX-License-Identifier: Apache-2.0

using GoogleAdk.Core.Agents;

namespace GoogleAdk.Dev.Server;

/// <summary>
/// Builds a JSON-friendly graph representation of an agent hierarchy.
/// Used by the dev UI to visualize agent topology.
/// </summary>
public static class AgentGraphBuilder
{
    public static AgentGraphNode BuildGraph(BaseAgent agent)
    {
        return BuildNode(agent);
    }

    private static AgentGraphNode BuildNode(BaseAgent agent)
    {
        var node = new AgentGraphNode
        {
            Name = agent.Name,
            Description = agent.Description,
            Type = agent.GetType().Name,
        };

        // Add tools for LlmAgent
        if (agent is LlmAgent llm)
        {
            foreach (var tool in llm.Tools)
            {
                node.Tools.Add(new AgentGraphTool
                {
                    Name = tool.Name,
                    Description = tool.Description,
                });
            }
        }

        // Recurse into sub-agents
        foreach (var sub in agent.SubAgents)
        {
            node.Children.Add(BuildNode(sub));
        }

        return node;
    }
}

public class AgentGraphNode
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = string.Empty;
    public List<AgentGraphTool> Tools { get; set; } = new();
    public List<AgentGraphNode> Children { get; set; } = new();
}

public class AgentGraphTool
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
