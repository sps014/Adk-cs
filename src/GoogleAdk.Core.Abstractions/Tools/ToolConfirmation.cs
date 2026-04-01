namespace GoogleAdk.Core.Abstractions.Tools;

/// <summary>
/// Represents a tool confirmation request.
/// </summary>
public class ToolConfirmation
{
    /// <summary>
    /// The function call ID that requires confirmation.
    /// </summary>
    public string FunctionCallId { get; set; } = string.Empty;

    /// <summary>
    /// Whether the tool confirmation was accepted.
    /// </summary>
    public bool? Accepted { get; set; }
}
