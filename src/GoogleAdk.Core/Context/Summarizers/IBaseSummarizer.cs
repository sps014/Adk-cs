using GoogleAdk.Core.Abstractions.Events;

namespace GoogleAdk.Core.Context.Summarizers;

/// <summary>
/// Interface for summarizing a list of events into a single CompactedEvent.
/// </summary>
public interface IBaseSummarizer
{
    /// <summary>
    /// Summarizes the given events into a CompactedEvent.
    /// </summary>
    Task<Events.CompactedEvent> SummarizeAsync(List<Event> events);
}
