using GoogleAdk.Core.Abstractions.Events;
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Core.Agents;

namespace GoogleAdk.Core.Planning;

/// <summary>
/// Planner that applies model-native thinking configuration to the request.
/// </summary>
public sealed class BuiltInPlanner : IPlanner
{
    public ThinkingConfig? ThinkingConfig { get; }

    public BuiltInPlanner(ThinkingConfig? thinkingConfig = null)
    {
        ThinkingConfig = thinkingConfig;
    }

    public string? BuildPlanningInstruction(ReadonlyContext context, LlmRequest request)
    {
        if (ThinkingConfig == null)
            return null;

        request.Config ??= new GenerateContentConfig();
        request.Config.ThinkingConfig = ThinkingConfig;
        return null;
    }

    public List<Part>? ProcessPlanningResponse(AgentContext context, List<Part> responseParts) => null;
}
