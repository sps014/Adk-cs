using System.Text.Json.Serialization;

namespace GoogleAdk.Core.Abstractions.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Modality
{
    MODALITY_UNSPECIFIED,
    TEXT,
    IMAGE,
    AUDIO,
    VIDEO
}

/// <summary>
/// Speech configuration for live agents.
/// </summary>
public class SpeechConfig
{
    [JsonPropertyName("voiceConfig")]
    public VoiceConfig? VoiceConfig { get; set; }
}

public class VoiceConfig
{
    [JsonPropertyName("prebuiltVoiceConfig")]
    public PrebuiltVoiceConfig? PrebuiltVoiceConfig { get; set; }
}

public class PrebuiltVoiceConfig
{
    [JsonPropertyName("voiceName")]
    public string? VoiceName { get; set; }
}

/// <summary>
/// Audio transcription configuration.
/// </summary>
public class AudioTranscriptionConfig
{
    [JsonPropertyName("languageCodes")]
    public List<string>? LanguageCodes { get; set; }
}

/// <summary>
/// Determine whether start of speech event interrupts the model's response.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ActivityHandling
{
    ACTIVITY_HANDLING_UNSPECIFIED,
    START_OF_ACTIVITY_INTERRUPTS,
    NO_INTERRUPTION
}

/// <summary>
/// Define which input is included in the user's turn.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TurnCoverage
{
    TURN_COVERAGE_UNSPECIFIED,
    TURN_INCLUDES_ONLY_ACTIVITY,
    TURN_INCLUDES_ALL_INPUT
}

/// <summary>
/// Sensitivity of start of speech detection.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StartSensitivity
{
    START_SENSITIVITY_UNSPECIFIED,
    START_SENSITIVITY_HIGH,
    START_SENSITIVITY_LOW
}

/// <summary>
/// Sensitivity of end of speech detection.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EndSensitivity
{
    END_SENSITIVITY_UNSPECIFIED,
    END_SENSITIVITY_HIGH,
    END_SENSITIVITY_LOW
}

/// <summary>
/// Settings for automatic voice activity detection.
/// </summary>
public class AutomaticActivityDetection
{
    [JsonPropertyName("disabled")]
    public bool? Disabled { get; set; }

    [JsonPropertyName("startOfSpeechSensitivity")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public StartSensitivity? StartOfSpeechSensitivity { get; set; }

    [JsonPropertyName("endOfSpeechSensitivity")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EndSensitivity? EndOfSpeechSensitivity { get; set; }

    [JsonPropertyName("prefixPaddingMs")]
    public int? PrefixPaddingMs { get; set; }

    [JsonPropertyName("silenceDurationMs")]
    public int? SilenceDurationMs { get; set; }
}

/// <summary>
/// Sliding window parameters.
/// </summary>
public class SlidingWindow
{
    [JsonPropertyName("targetTokens")]
    public int? TargetTokens { get; set; }
}

/// <summary>
/// Realtime input configuration.
/// </summary>
public class RealtimeInputConfig
{
    [JsonPropertyName("automaticActivityDetection")]
    public AutomaticActivityDetection? AutomaticActivityDetection { get; set; }

    [JsonPropertyName("activityHandling")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ActivityHandling? ActivityHandling { get; set; }

    [JsonPropertyName("turnCoverage")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TurnCoverage? TurnCoverage { get; set; }
}

/// <summary>
/// Proactivity configuration.
/// </summary>
public class ProactivityConfig
{
    [JsonPropertyName("proactiveAudio")]
    public bool? ProactiveAudio { get; set; }
}

/// <summary>
/// Session resumption configuration.
/// </summary>
public class SessionResumptionConfig
{
    [JsonPropertyName("handle")]
    public string? Handle { get; set; }

    [JsonPropertyName("transparent")]
    public bool? Transparent { get; set; }
}

/// <summary>
/// Context window compression configuration.
/// </summary>
public class ContextWindowCompressionConfig
{
    [JsonPropertyName("triggerTokens")]
    public int? TriggerTokens { get; set; }

    [JsonPropertyName("slidingWindow")]
    public SlidingWindow? SlidingWindow { get; set; }
}
