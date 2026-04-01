using GoogleAdk.Core.Abstractions.Events;
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Core.Agents;

namespace GoogleAdk.Core.Tools;

/// <summary>
/// A built-in tool using Vertex AI Search (Discovery Engine API).
/// 
/// To dynamically customize the search configuration at runtime (e.g., set
/// filter based on user context), subclass this tool and override the
/// <see cref="BuildVertexAiSearchConfig"/> method.
/// </summary>
public class VertexAiSearchTool : BaseTool
{
    public string? DataStoreId { get; }
    public List<VertexAiSearchDataStoreSpec>? DataStoreSpecs { get; }
    public string? SearchEngineId { get; }
    public string? Filter { get; }
    public int? MaxResults { get; }
    public bool BypassMultiToolsLimit { get; }

    /// <summary>
    /// Initializes the Vertex AI Search tool.
    /// </summary>
    /// <param name="dataStoreId">The Vertex AI search data store resource ID in the format of "projects/{project}/locations/{location}/collections/{collection}/dataStores/{dataStore}".</param>
    /// <param name="dataStoreSpecs">Specifications that define the specific DataStores to be searched. It should only be set if engine is used.</param>
    /// <param name="searchEngineId">The Vertex AI search engine resource ID in the format of "projects/{project}/locations/{location}/collections/{collection}/engines/{engine}".</param>
    /// <param name="filter">The filter to apply to the search results.</param>
    /// <param name="maxResults">The maximum number of results to return.</param>
    /// <param name="bypassMultiToolsLimit">Whether to bypass the multi tools limitation, so that the tool can be used with other tools in the same agent.</param>
    public VertexAiSearchTool(
        string? dataStoreId = null,
        List<VertexAiSearchDataStoreSpec>? dataStoreSpecs = null,
        string? searchEngineId = null,
        string? filter = null,
        int? maxResults = null,
        bool bypassMultiToolsLimit = false)
        : base("vertex_ai_search", "vertex_ai_search")
    {
        if ((dataStoreId == null && searchEngineId == null) || (dataStoreId != null && searchEngineId != null))
        {
            throw new ArgumentException("Either dataStoreId or searchEngineId must be specified.");
        }
        if (dataStoreSpecs != null && searchEngineId == null)
        {
            throw new ArgumentException("searchEngineId must be specified if dataStoreSpecs is specified.");
        }

        DataStoreId = dataStoreId;
        DataStoreSpecs = dataStoreSpecs;
        SearchEngineId = searchEngineId;
        Filter = filter;
        MaxResults = maxResults;
        BypassMultiToolsLimit = bypassMultiToolsLimit;
    }

    /// <summary>
    /// Builds the VertexAISearch configuration.
    /// Override this method in a subclass to dynamically customize the search
    /// configuration based on the context (e.g., set filter based on session state).
    /// </summary>
    /// <param name="context">The agent context with access to state and session info.</param>
    protected virtual VertexAiSearchConfig BuildVertexAiSearchConfig(AgentContext context)
    {
        return new VertexAiSearchConfig
        {
            Datastore = DataStoreId,
            DataStoreSpecs = DataStoreSpecs,
            Engine = SearchEngineId,
            Filter = Filter,
            MaxResults = MaxResults
        };
    }

    public override Task<object?> RunAsync(Dictionary<string, object?> args, AgentContext context)
    {
        // This is a built-in tool handled by the LLM backend directly.
        // RunAsync should typically not be called by the orchestrator for backend retrieval tools
        // unless it's mock-executed.
        throw new InvalidOperationException("VertexAiSearchTool is executed by the model backend, not locally.");
    }

    public override Task ProcessLlmRequestAsync(AgentContext context, LlmRequest llmRequest)
    {
        llmRequest.Config ??= new GenerateContentConfig();
        llmRequest.Config.Tools ??= new List<ToolDeclaration>();

        var vertexAiSearchConfig = BuildVertexAiSearchConfig(context);

        llmRequest.Config.Tools.Add(new ToolDeclaration
        {
            Retrieval = new RetrievalConfig
            {
                VertexAiSearch = vertexAiSearchConfig
            }
        });

        return Task.CompletedTask;
    }
}
