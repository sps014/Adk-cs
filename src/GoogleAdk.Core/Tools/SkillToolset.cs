using GoogleAdk.Core.Agents;

namespace GoogleAdk.Core.Tools;

/// <summary>
/// Placeholder skill toolset.
/// </summary>
public sealed class SkillToolset : BaseToolset
{
    public override Task<IReadOnlyList<BaseTool>> GetToolsAsync(AgentContext? context = null)
        => Task.FromResult<IReadOnlyList<BaseTool>>(new List<BaseTool>());
}
