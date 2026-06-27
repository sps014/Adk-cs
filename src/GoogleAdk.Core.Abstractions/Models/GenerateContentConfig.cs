using System.Text.Json.Serialization;

namespace GoogleAdk.Core.Abstractions.Models;

/// <summary>
/// Configuration for content generation requests.
/// </summary>
public class GenerateContentConfig
{
    [JsonPropertyName("systemInstruction")]
    public string? SystemInstruction { get; set; }

    [JsonPropertyName("tools")]
    public List<ToolDeclaration>? Tools { get; set; }

    [JsonPropertyName("responseSchema")]
    public Dictionary<string, object?>? ResponseSchema { get; set; }

    [JsonPropertyName("responseMimeType")]
    public string? ResponseMimeType { get; set; }

    [JsonPropertyName("thinkingConfig")]
    public ThinkingConfig? ThinkingConfig { get; set; }

    [JsonPropertyName("safetySettings")]
    public List<SafetySetting>? SafetySettings { get; set; }

    [JsonPropertyName("temperature")]
    public double? Temperature { get; set; }

    [JsonPropertyName("topP")]
    public double? TopP { get; set; }

    [JsonPropertyName("topK")]
    public int? TopK { get; set; }

    [JsonPropertyName("maxOutputTokens")]
    public int? MaxOutputTokens { get; set; }

    [JsonPropertyName("candidateCount")]
    public int? CandidateCount { get; set; }

    [JsonPropertyName("stopSequences")]
    public List<string>? StopSequences { get; set; }

    [JsonPropertyName("responseModalities")]
    public List<Modality>? ResponseModalities { get; set; }

    [JsonPropertyName("speechConfig")]
    public SpeechConfig? SpeechConfig { get; set; }
}

/// <summary>
/// Configuration for model thinking features.
/// </summary>
public class ThinkingConfig
{
    [JsonPropertyName("thinkingBudget")]
    public int? ThinkingBudget { get; set; }

    [JsonPropertyName("includeThoughts")]
    public bool? IncludeThoughts { get; set; }
}

/// <summary>
/// Safety setting, affecting the safety-related filters.
/// </summary>
public class SafetySetting
{
    [JsonPropertyName("category")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public HarmCategory Category { get; set; }

    [JsonPropertyName("threshold")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public HarmBlockThreshold Threshold { get; set; }
}

/// <summary>
/// The category of a rating.
/// </summary>
public enum HarmCategory
{
    HARM_CATEGORY_UNSPECIFIED,
    HARM_CATEGORY_HATE_SPEECH,
    HARM_CATEGORY_DANGEROUS_CONTENT,
    HARM_CATEGORY_HARASSMENT,
    HARM_CATEGORY_SEXUALLY_EXPLICIT
}

/// <summary>
/// Block at and beyond a specified harm probability.
/// </summary>
public enum HarmBlockThreshold
{
    HARM_BLOCK_THRESHOLD_UNSPECIFIED,
    BLOCK_LOW_AND_ABOVE,
    BLOCK_MEDIUM_AND_ABOVE,
    BLOCK_ONLY_HIGH,
    BLOCK_NONE,
    OFF
}
