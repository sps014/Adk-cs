using GoogleAdk.Core.Abstractions.Events;
using GoogleAdk.Core.Abstractions.Models;

namespace GoogleAdk.Models.Gemini;

/// <summary>
/// Gemini live connection wrapper. Currently backed by StreamingLlmConnection.
/// </summary>
public sealed class GeminiLiveConnection : BaseLlmConnection
{
    private readonly StreamingLlmConnection _inner;

    public GeminiLiveConnection(BaseLlm llm, LlmRequest request)
    {
        _inner = new StreamingLlmConnection(llm, request);
    }

    public override Task SendHistoryAsync(IEnumerable<Content> history, CancellationToken cancellationToken = default)
        => _inner.SendHistoryAsync(history, cancellationToken);

    public override Task SendContentAsync(Content content, CancellationToken cancellationToken = default)
        => _inner.SendContentAsync(content, cancellationToken);

    public override Task SendRealtimeAsync(Part part, CancellationToken cancellationToken = default)
        => _inner.SendRealtimeAsync(part, cancellationToken);

    public override IAsyncEnumerable<LlmResponse> ReceiveAsync(CancellationToken cancellationToken = default)
        => _inner.ReceiveAsync(cancellationToken);

    public override ValueTask DisposeAsync()
        => _inner.DisposeAsync();
}
