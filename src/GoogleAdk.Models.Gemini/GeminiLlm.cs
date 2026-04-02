using GenerativeAI;
using GenerativeAI.Microsoft;
using GenerativeAI.Types;
using GenerativeAI.Types.RagEngine;
using GoogleAdk.Core.Abstractions.Events;
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Models.Meai;
using System.Runtime.CompilerServices;

namespace GoogleAdk.Models.Gemini;

/// <summary>
/// A specialized LLM wrapper for Gemini models that supports native Gemini features
/// like Google Search and Vertex AI Search (Retrieval) which are not supported
/// by standard MEAI ChatOptions.
/// </summary>
public class GeminiLlm : MeaiLlm
{
    private readonly GenerativeAIChatClient _client;

    public GeminiLlm(string model, GenerativeAIChatClient client) : base(model, client)
    {
        _client = client;
    }

    private static readonly System.Reflection.PropertyInfo? RetrievalToolProp = typeof(GenerativeModel).GetProperty("RetrievalTool", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

    static void SetRetrievalTool(GenerativeModel obj, GenerativeAI.Types.Tool? value)
    {
        RetrievalToolProp?.SetValue(obj, value);
    }

    public override async IAsyncEnumerable<LlmResponse> GenerateContentAsync(
        LlmRequest llmRequest, 
        bool stream = false, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Extract the underlying GenerativeModel to configure native tools
        var genModel = _client.model;

        if (genModel != null && llmRequest.Config?.Tools != null)
        {
            // Reset specialized tool state
            genModel.UseGoogleSearch = false;
            SetRetrievalTool(genModel, null);

            foreach (var toolDecl in llmRequest.Config.Tools)
            {
                if (toolDecl.GoogleSearch != null || toolDecl.GoogleSearchRetrieval != null)
                {
                    genModel.UseGoogleSearch = true;
                }
                
                if (toolDecl.Retrieval?.VertexAiSearch != null)
                {
                    var vs = toolDecl.Retrieval.VertexAiSearch;
                    var specs = vs.DataStoreSpecs?.Select(s => new VertexAISearchDataStoreSpec { DataStore = s.DataStore }).ToList();
                    
                    var toolObj = new GenerativeAI.Types.Tool
                    {
                        Retrieval = new VertexRetrievalTool
                        {
                            VertexAiSearch = new VertexAISearch
                            {
                                Datastore = vs.Datastore,
                                Engine = vs.Engine,
                                Filter = vs.Filter,
                                DataStoreSpecs = specs,
                                MaxResults = vs.MaxResults
                            }
                        }
                    };
                    SetRetrievalTool(genModel, toolObj);
                }
            }
        }

        await foreach (var resp in base.GenerateContentAsync(llmRequest, stream, cancellationToken))
        {
            if (resp.RawRepresentation is GenerateContentResponse raw)
            {
                var gm = raw.Candidates?.FirstOrDefault()?.GroundingMetadata;
                if (gm != null)
                {
                    resp.GroundingMetadata = new GoogleAdk.Core.Abstractions.Models.GroundingMetadata
                    {
                        WebSearchQueries = gm.WebSearchQueries,
                        SearchEntryPoint = gm.SearchEntryPoint == null ? null : new Dictionary<string, object?>
                        {
                            { "renderedContent", gm.SearchEntryPoint.RenderedContent }
                        },
                        GroundingChunks = gm.GroundingChunks?.Select(c => new Dictionary<string, object?>
                        {
                            { "web", c.Web },
                            { "retrievedContext", c.RetrievedContext }
                        }).ToList(),
                        GroundingSupports = gm.GroundingSupports?.Select(s => new Dictionary<string, object?>
                        {
                            { "segment", s.Segment },
                            { "groundingChunkIndices", s.GroundingChunkIndices },
                            { "confidenceScores", s.ConfidenceScores }
                        }).ToList()
                    };
                }
            }
            yield return resp;
        }
    }

    public override Task<BaseLlmConnection> ConnectAsync(LlmRequest llmRequest)
    {
        BaseLlmConnection connection = new GeminiLiveConnection(this, llmRequest);
        return Task.FromResult(connection);
    }
}
