namespace GoogleAdk.Core.Abstractions.Events;

/// <summary>
/// UI widget descriptor for rich client rendering.
/// </summary>
public sealed class UiWidget
{
    public string Id { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public Dictionary<string, object?> Payload { get; set; } = new();
}
