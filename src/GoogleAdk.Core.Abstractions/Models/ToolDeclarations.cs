using System.Text.Json.Serialization;

namespace GoogleAdk.Core.Abstractions.Models;

/// <summary>
/// A function declaration that a tool exposes to the model.
/// </summary>
public class FunctionDeclaration
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("parameters")]
    public Schema? Parameters { get; set; }
}

/// <summary>
/// The Schema object allows the definition of input and output data types.
/// </summary>
public class Schema
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("format")]
    public string? Format { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("properties")]
    public Dictionary<string, Schema>? Properties { get; set; }

    [JsonPropertyName("items")]
    public Schema? Items { get; set; }

    [JsonPropertyName("enum")]
    public List<string>? Enum { get; set; }

    [JsonPropertyName("required")]
    public List<string>? Required { get; set; }

    public static implicit operator Schema?(Dictionary<string, object?>? dict)
    {
        if (dict == null) return null;
        var json = System.Text.Json.JsonSerializer.Serialize(dict);
        return System.Text.Json.JsonSerializer.Deserialize<Schema>(json);
    }
}

/// <summary>
/// A tool declaration sent to the model.
/// </summary>
public class ToolDeclaration
{
    [JsonPropertyName("functionDeclarations")]
    public List<FunctionDeclaration>? FunctionDeclarations { get; set; }

    [JsonPropertyName("googleSearch")]
    public GoogleSearchConfig? GoogleSearch { get; set; }

    [JsonPropertyName("googleSearchRetrieval")]
    public GoogleSearchRetrievalConfig? GoogleSearchRetrieval { get; set; }

    [JsonPropertyName("retrieval")]
    public RetrievalConfig? Retrieval { get; set; }

    [JsonPropertyName("urlContext")]
    public UrlContextConfig? UrlContext { get; set; }

    [JsonPropertyName("enterpriseWebSearch")]
    public EnterpriseWebSearchConfig? EnterpriseWebSearch { get; set; }

    [JsonPropertyName("googleMaps")]
    public GoogleMapsConfig? GoogleMaps { get; set; }

    [JsonPropertyName("codeExecution")]
    public CodeExecutionConfig? CodeExecution { get; set; }
}

public class CodeExecutionConfig
{
}

public class GoogleSearchConfig
{
}

public class GoogleSearchRetrievalConfig
{
    [JsonPropertyName("dynamicRetrievalConfig")]
    public DynamicRetrievalConfig? DynamicRetrievalConfig { get; set; }
}

public class DynamicRetrievalConfig
{
    [JsonPropertyName("mode")]
    public string? Mode { get; set; }

    [JsonPropertyName("dynamicThreshold")]
    public double? DynamicThreshold { get; set; }
}

public class UrlContextConfig
{
}

public class EnterpriseWebSearchConfig
{
}

public class GoogleMapsConfig
{
}

/// <summary>
/// Configuration for retrieval tools (like Vertex AI Search).
/// </summary>
public class RetrievalConfig
{
    [JsonPropertyName("vertexAiSearch")]
    public VertexAiSearchConfig? VertexAiSearch { get; set; }

    [JsonPropertyName("vertexRagStore")]
    public VertexRagStoreConfig? VertexRagStore { get; set; }
}

/// <summary>
/// Configuration for Vertex RAG Store.
/// </summary>
public class VertexRagStoreConfig
{
    [JsonPropertyName("ragCorpora")]
    public List<string>? RagCorpora { get; set; }

    [JsonPropertyName("ragResources")]
    public List<VertexAiSearchDataStoreSpec>? RagResources { get; set; }

    [JsonPropertyName("similarityTopK")]
    public int? SimilarityTopK { get; set; }

    [JsonPropertyName("vectorDistanceThreshold")]
    public double? VectorDistanceThreshold { get; set; }
}

/// <summary>
/// Configuration for Vertex AI Search retrieval.
/// </summary>
public class VertexAiSearchConfig
{
    [JsonPropertyName("datastore")]
    public string? Datastore { get; set; }

    [JsonPropertyName("engine")]
    public string? Engine { get; set; }

    [JsonPropertyName("filter")]
    public string? Filter { get; set; }

    [JsonPropertyName("maxResults")]
    public int? MaxResults { get; set; }

    [JsonPropertyName("dataStoreSpecs")]
    public List<VertexAiSearchDataStoreSpec>? DataStoreSpecs { get; set; }
}

/// <summary>
/// A data store specification for Vertex AI Search.
/// </summary>
public class VertexAiSearchDataStoreSpec
{
    [JsonPropertyName("dataStore")]
    public string? DataStore { get; set; }
}
