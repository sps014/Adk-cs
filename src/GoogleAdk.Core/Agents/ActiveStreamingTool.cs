namespace GoogleAdk.Core.Agents;

/// <summary>
/// Holds a running streaming tool task and its optional live input queue.
/// </summary>
public sealed class ActiveStreamingTool
{
    public Task? Task { get; init; }
    public LiveRequestQueue? Stream { get; init; }
}
