using System.Text.Json.Serialization;

namespace GoogleAdk.Core.Abstractions.Models;

/// <summary>
/// Usage metadata from a content generation response.
/// </summary>
public class UsageMetadata
{
    [JsonPropertyName("promptTokenCount")]
    public int? PromptTokenCount { get; set; }

    [JsonPropertyName("candidatesTokenCount")]
    public int? CandidatesTokenCount { get; set; }

    [JsonPropertyName("totalTokenCount")]
    public int? TotalTokenCount { get; set; }
}

/// <summary>
/// Grounding metadata from the response.
/// </summary>
public class GroundingMetadata
{
    [JsonPropertyName("webSearchQueries")]
    public List<string>? WebSearchQueries { get; set; }

    [JsonPropertyName("searchEntryPoint")]
    public SearchEntryPoint? SearchEntryPoint { get; set; }

    [JsonPropertyName("groundingChunks")]
    public List<GroundingChunk>? GroundingChunks { get; set; }

    [JsonPropertyName("groundingSupports")]
    public List<GroundingSupport>? GroundingSupports { get; set; }
}

public class SearchEntryPoint
{
    [JsonPropertyName("renderedContent")]
    public string? RenderedContent { get; set; }
}

public class GroundingChunk
{
    [JsonPropertyName("web")]
    public WebGroundingChunk? Web { get; set; }

    [JsonPropertyName("retrievedContext")]
    public RetrievedContextGroundingChunk? RetrievedContext { get; set; }
}

public class WebGroundingChunk
{
    [JsonPropertyName("uri")]
    public string? Uri { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }
}

public class RetrievedContextGroundingChunk
{
    [JsonPropertyName("uri")]
    public string? Uri { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }
}

public class GroundingSupport
{
    [JsonPropertyName("segment")]
    public Segment? Segment { get; set; }

    [JsonPropertyName("groundingChunkIndices")]
    public List<int>? GroundingChunkIndices { get; set; }
}

public class Segment
{
    [JsonPropertyName("startIndex")]
    public int? StartIndex { get; set; }

    [JsonPropertyName("endIndex")]
    public int? EndIndex { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }
}

/// <summary>
/// Citation metadata from the response.
/// </summary>
public class CitationMetadata
{
    [JsonPropertyName("citations")]
    public List<Citation>? Citations { get; set; }
}

public class Citation
{
    [JsonPropertyName("startIndex")]
    public int? StartIndex { get; set; }

    [JsonPropertyName("endIndex")]
    public int? EndIndex { get; set; }

    [JsonPropertyName("uri")]
    public string? Uri { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("license")]
    public string? License { get; set; }

    [JsonPropertyName("publicationDate")]
    public DateInfo? PublicationDate { get; set; }
}

public class DateInfo
{
    [JsonPropertyName("year")]
    public int? Year { get; set; }

    [JsonPropertyName("month")]
    public int? Month { get; set; }

    [JsonPropertyName("day")]
    public int? Day { get; set; }
}

/// <summary>
/// Transcription data for audio.
/// </summary>
public class Transcription
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}
