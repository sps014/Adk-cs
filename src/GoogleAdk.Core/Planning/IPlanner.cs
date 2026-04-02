using GoogleAdk.Core.Abstractions.Events;
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Core.Agents;

namespace GoogleAdk.Core.Planning;

/// <summary>
/// Planner interface for optional planning behavior in LLM flows.
/// </summary>
public interface IPlanner
{
    /// <summary>
    /// Builds additional planning instruction text to append to the request.
    /// Return null to skip instruction injection.
    /// </summary>
    string? BuildPlanningInstruction(ReadonlyContext context, LlmRequest request);

    /// <summary>
    /// Post-processes planning-related parts in the model response.
    /// Return null to leave parts unchanged.
    /// </summary>
    List<Part>? ProcessPlanningResponse(AgentContext context, List<Part> responseParts);
}
