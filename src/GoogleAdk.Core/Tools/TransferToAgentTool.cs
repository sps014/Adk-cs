using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Core.Agents;

namespace GoogleAdk.Core.Tools;

/// <summary>
/// Tool for transferring control to another agent.
/// </summary>
public sealed class TransferToAgentTool : BaseTool
{
    private readonly IReadOnlyList<string> _agentNames;

    public TransferToAgentTool(IReadOnlyList<string> agentNames)
        : base("transfer_to_agent",
            "Transfer the question to another agent that is better suited to answer it.")
    {
        _agentNames = agentNames;
    }

    public override FunctionDeclaration? GetDeclaration()
    {
        return new FunctionDeclaration
        {
            Name = Name,
            Description = Description,
            Parameters = new Dictionary<string, object?>
            {
                ["type"] = "object",
                ["properties"] = new Dictionary<string, object?>
                {
                    ["agentName"] = new Dictionary<string, object?>
                    {
                        ["type"] = "string",
                        ["enum"] = _agentNames.ToArray(),
                        ["description"] = "The agent name to transfer to."
                    }
                },
                ["required"] = new[] { "agentName" }
            }
        };
    }

    public override Task<object?> RunAsync(Dictionary<string, object?> args, AgentContext context)
    {
        var agentName = args.GetValueOrDefault("agentName")?.ToString() ?? "";
        context.EventActions.TransferToAgent = agentName;
        return Task.FromResult<object?>("Transfer queued");
    }
}
