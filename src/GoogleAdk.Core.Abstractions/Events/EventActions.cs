using GoogleAdk.Core.Abstractions.Auth;
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Core.Abstractions.Tools;

namespace GoogleAdk.Core.Abstractions.Events;

/// <summary>
/// The compaction of a range of events into a single condensed content.
/// </summary>
public class EventCompaction
{
    /// <summary>The start timestamp of the compacted events (Unix milliseconds).</summary>
    public long StartTimestamp { get; set; }

    /// <summary>The end timestamp of the compacted events (Unix milliseconds).</summary>
    public long EndTimestamp { get; set; }

    /// <summary>The compacted content of the events.</summary>
    public Content? CompactedContent { get; set; }
}

/// <summary>
/// Represents the actions attached to an event.
/// </summary>
public class EventActions
{
    /// <summary>
    /// If true, it won't call model to summarize function response.
    /// Only used for function_response event.
    /// </summary>
    public bool? SkipSummarization { get; set; }

    /// <summary>
    /// Indicates that the event is updating the state with the given delta.
    /// </summary>
    public Dictionary<string, object?> StateDelta { get; set; } = new();

    /// <summary>
    /// Indicates that the event is updating an artifact. Key is the filename, value is the version.
    /// </summary>
    public Dictionary<string, int> ArtifactDelta { get; set; } = new();

    /// <summary>
    /// If set, the event transfers to the specified agent.
    /// </summary>
    public string? TransferToAgent { get; set; }

    /// <summary>
    /// The agent is escalating to a higher level agent.
    /// </summary>
    public bool? Escalate { get; set; }

    /// <summary>
    /// Authentication configurations requested by tool responses.
    /// Keys: The function call id. Values: The requested auth config.
    /// </summary>
    public Dictionary<string, AuthConfig> RequestedAuthConfigs { get; set; } = new();

    /// <summary>
    /// A dict of tool confirmation requested by this event, keyed by the function call id.
    /// </summary>
    public Dictionary<string, ToolConfirmation> RequestedToolConfirmations { get; set; } = new();

    /// <summary>
    /// Custom metadata for the event actions.
    /// </summary>
    public Dictionary<string, object?>? CustomMetadata { get; set; }

    /// <summary>
    /// The compaction of the events, if this event represents a compaction.
    /// </summary>
    public EventCompaction? Compaction { get; set; }

    /// <summary>
    /// If true, the current agent has finished its current run. Note that there
    /// can be multiple events with EndOfAgent=true for the same agent within one
    /// invocation when there is a loop. This should only be set by the workflow.
    /// </summary>
    public bool? EndOfAgent { get; set; }

    /// <summary>
    /// The agent state at the current event, used for checkpoint and resume.
    /// This should only be set by the workflow.
    /// </summary>
    public Dictionary<string, object?>? AgentState { get; set; }

    /// <summary>
    /// The invocation id to rewind to. Only set for rewind events.
    /// </summary>
    public string? RewindBeforeInvocationId { get; set; }

    /// <summary>
    /// Route or list of routes for workflow graph edge matching.
    /// </summary>
    public object? Route { get; set; }

    /// <summary>
    /// The model response structured output.
    /// </summary>
    public object? SetModelResponse { get; set; }

    /// <summary>
    /// UI widgets requested for rendering.
    /// </summary>
    public List<UiWidget> RenderUiWidgets { get; set; } = new();

    /// <summary>
    /// Creates a new EventActions with default values.
    /// </summary>
    public static EventActions Create(Action<EventActions>? configure = null)
    {
        var actions = new EventActions();
        configure?.Invoke(actions);
        return actions;
    }

    /// <summary>
    /// Merges a list of EventActions into a single EventActions.
    /// Dictionaries are merged by adding all properties. For scalar properties, last one wins.
    /// </summary>
    public static EventActions Merge(IEnumerable<EventActions?> sources, EventActions? target = null)
    {
        var result = new EventActions();

        if (target != null)
        {
            CopyScalars(target, result);
            MergeDictionaries(target, result);
        }

        foreach (var source in sources)
        {
            if (source == null) continue;

            foreach (var kv in source.StateDelta)
                result.StateDelta[kv.Key] = kv.Value;

            foreach (var kv in source.ArtifactDelta)
                result.ArtifactDelta[kv.Key] = kv.Value;

            foreach (var kv in source.RequestedAuthConfigs)
                result.RequestedAuthConfigs[kv.Key] = kv.Value;

            foreach (var kv in source.RequestedToolConfirmations)
                result.RequestedToolConfirmations[kv.Key] = kv.Value;

            if (source.RenderUiWidgets.Count > 0)
                result.RenderUiWidgets.AddRange(source.RenderUiWidgets);

            CopyScalars(source, result);
        }

        return result;
    }

    private static void CopyScalars(EventActions source, EventActions target)
    {
        if (source.SkipSummarization.HasValue)
            target.SkipSummarization = source.SkipSummarization;
        if (source.TransferToAgent != null)
            target.TransferToAgent = source.TransferToAgent;
        if (source.Escalate.HasValue)
            target.Escalate = source.Escalate;
        if (source.Compaction != null)
            target.Compaction = source.Compaction;
        if (source.EndOfAgent.HasValue)
            target.EndOfAgent = source.EndOfAgent;
        if (source.AgentState != null)
            target.AgentState = source.AgentState;
        if (source.RewindBeforeInvocationId != null)
            target.RewindBeforeInvocationId = source.RewindBeforeInvocationId;
        if (source.Route != null)
            target.Route = source.Route;
        if (source.SetModelResponse != null)
            target.SetModelResponse = source.SetModelResponse;
    }

    private static void MergeDictionaries(EventActions source, EventActions target)
    {
        foreach (var kv in source.StateDelta)
            target.StateDelta[kv.Key] = kv.Value;
        foreach (var kv in source.ArtifactDelta)
            target.ArtifactDelta[kv.Key] = kv.Value;
        foreach (var kv in source.RequestedAuthConfigs)
            target.RequestedAuthConfigs[kv.Key] = kv.Value;
        foreach (var kv in source.RequestedToolConfirmations)
            target.RequestedToolConfirmations[kv.Key] = kv.Value;
        if (source.RenderUiWidgets.Count > 0)
            target.RenderUiWidgets.AddRange(source.RenderUiWidgets);
    }
}
