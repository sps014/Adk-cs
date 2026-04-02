using System.Text.Json;
using GoogleAdk.Core.Abstractions.Events;
using GoogleAdk.Core.Agents;

namespace GoogleAdk.Core.Plugins;

/// <summary>
/// Writes debug logs for events to a file.
/// </summary>
public sealed class DebugLoggingPlugin : BasePlugin
{
    private readonly string _logPath;
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public DebugLoggingPlugin(string logPath) : base(nameof(DebugLoggingPlugin))
    {
        _logPath = logPath;
    }

    public override async Task<Event?> OnEventCallbackAsync(InvocationContext invocationContext, Event evt)
    {
        var json = JsonSerializer.Serialize(evt, _options);
        await File.AppendAllTextAsync(_logPath, json + Environment.NewLine);
        return null;
    }
}
