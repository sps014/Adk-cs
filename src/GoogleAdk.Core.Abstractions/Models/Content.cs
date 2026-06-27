using System.Text.Json.Serialization;

namespace GoogleAdk.Core.Abstractions.Models;

/// <summary>
/// Represents a part of a content message (text, function call, function response, inline data, etc.).
/// </summary>
public class Part
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("functionCall")]
    public FunctionCall? FunctionCall { get; set; }

    [JsonPropertyName("functionResponse")]
    public FunctionResponse? FunctionResponse { get; set; }

    [JsonPropertyName("inlineData")]
    public InlineData? InlineData { get; set; }

    [JsonPropertyName("fileData")]
    public FileData? FileData { get; set; }

    [JsonPropertyName("codeExecutionResult")]
    public CodeExecutionResult? CodeExecutionResult { get; set; }

    [JsonPropertyName("executableCode")]
    public ExecutableCode? ExecutableCode { get; set; }

    [JsonPropertyName("thought")]
    public bool? Thought { get; set; }
}

/// <summary>
/// Represents a function call from the model.
/// </summary>
public class FunctionCall
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("args")]
    public Dictionary<string, object?>? Args { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }
}

/// <summary>
/// Represents a function response to the model.
/// </summary>
public class FunctionResponse
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("response")]
    public Dictionary<string, object?>? Response { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }
}

/// <summary>
/// Inline binary data (e.g., images).
/// </summary>
public class InlineData
{
    [JsonPropertyName("mimeType")]
    public string MimeType { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public string Data { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }
}

/// <summary>
/// File reference data (URI-based).
/// </summary>
public class FileData
{
    [JsonPropertyName("fileUri")]
    public string? FileUri { get; set; }

    [JsonPropertyName("mimeType")]
    public string? MimeType { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

/// <summary>
/// Executable code content.
/// </summary>
public class ExecutableCode
{
    [JsonPropertyName("language")]
    public string? Language { get; set; }

    [JsonPropertyName("code")]
    public string? Code { get; set; }
}

/// <summary>
/// Result from code execution.
/// </summary>
public class CodeExecutionResult
{
    [JsonPropertyName("outcome")]
    public string? Outcome { get; set; }

    [JsonPropertyName("output")]
    public string? Output { get; set; }
}

/// <summary>
/// Represents content in a conversation (a message from user or model).
/// </summary>
public class Content
{
    [JsonPropertyName("role")]
    public string? Role { get; set; }

    [JsonPropertyName("parts")]
    public List<Part>? Parts { get; set; }
}
