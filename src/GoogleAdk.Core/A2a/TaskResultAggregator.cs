namespace GoogleAdk.Core.A2a;

/// <summary>
/// Aggregates the overall task state across an A2A event stream, mirroring
/// adk-python's <c>TaskResultAggregator</c>. The reported state follows a priority
/// order so that an interrupting/failing state observed at any point in the stream
/// is not masked by a later "working" update:
/// <c>failed</c> &gt; <c>auth-required</c> &gt; <c>input-required</c> &gt; <c>working</c>.
/// </summary>
public sealed class TaskResultAggregator
{
    /// <summary>The aggregated task state, or null if no state has been observed.</summary>
    public string? TaskState { get; private set; }

    /// <summary>The message attached to the highest-priority observed state, if any.</summary>
    public Message? TaskStatusMessage { get; private set; }

    /// <summary>
    /// Processes a single A2A event, updating the aggregated state.
    /// </summary>
    public void Process(IA2aEvent evt)
    {
        var (state, message) = evt switch
        {
            TaskStatusUpdateEvent tsu => (tsu.Status.State, tsu.Status.Message),
            A2aTask task => (task.Status.State, task.Status.Message),
            _ => (null, (Message?)null),
        };

        if (state == null)
            return;

        if (state == GoogleAdk.Core.A2a.TaskState.Failed)
        {
            SetState(GoogleAdk.Core.A2a.TaskState.Failed, message);
        }
        else if (state == GoogleAdk.Core.A2a.TaskState.AuthRequired
                 && TaskState != GoogleAdk.Core.A2a.TaskState.Failed)
        {
            SetState(GoogleAdk.Core.A2a.TaskState.AuthRequired, message);
        }
        else if (state == GoogleAdk.Core.A2a.TaskState.InputRequired
                 && TaskState != GoogleAdk.Core.A2a.TaskState.Failed
                 && TaskState != GoogleAdk.Core.A2a.TaskState.AuthRequired)
        {
            SetState(GoogleAdk.Core.A2a.TaskState.InputRequired, message);
        }
        else if (TaskState == null)
        {
            SetState(GoogleAdk.Core.A2a.TaskState.Working, message);
        }
    }

    private void SetState(string state, Message? message)
    {
        TaskState = state;
        if (message != null)
            TaskStatusMessage = message;
    }
}
