using GoogleAdk.Core.Abstractions.Events;
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Core.Agents;

namespace GoogleAdk.Core.Tools;

public sealed class DiscoveryEngineSearchTool : BaseTool
{
    public string? Datastore { get; }
    public string? Engine { get; }

    public DiscoveryEngineSearchTool(string? datastore = null, string? engine = null)
        : base("discovery_engine_search", "Discovery Engine Search Tool")
    {
        Datastore = datastore;
        Engine = engine;
    }

    public override Task<object?> RunAsync(Dictionary<string, object?> args, AgentContext context)
        => Task.FromResult<object?>(null);

    public override Task ProcessLlmRequestAsync(AgentContext context, LlmRequest llmRequest)
    {
        llmRequest.Config ??= new GenerateContentConfig();
        llmRequest.Config.Tools ??= new List<ToolDeclaration>();
        llmRequest.Config.Tools.Add(new ToolDeclaration
        {
            GoogleSearchRetrieval = new Dictionary<string, object?>
            {
                ["datastore"] = Datastore,
                ["engine"] = Engine
            }
        });
        return Task.CompletedTask;
    }
}
