using System.Threading.Channels;
using GoogleAdk.Core.Abstractions.Models;

namespace GoogleAdk.Core.Agents;

public enum LiveActivity
{
    Start,
    End
}

public sealed class LiveRequest
{
    public Content? Content { get; init; }
    public Part? RealtimePart { get; init; }
    public LiveActivity? Activity { get; init; }
    public bool Close { get; init; }
}

/// <summary>
/// Queue for live bidirectional requests.
/// </summary>
public sealed class LiveRequestQueue
{
    private readonly Channel<LiveRequest> _channel = Channel.CreateUnbounded<LiveRequest>();

    public ValueTask EnqueueAsync(LiveRequest request, CancellationToken cancellationToken = default)
        => _channel.Writer.WriteAsync(request, cancellationToken);

    public ValueTask SendContentAsync(Content content, CancellationToken cancellationToken = default)
        => EnqueueAsync(new LiveRequest { Content = content }, cancellationToken);

    public ValueTask SendRealtimeAsync(Part part, CancellationToken cancellationToken = default)
        => EnqueueAsync(new LiveRequest { RealtimePart = part }, cancellationToken);

    public ValueTask SendActivityAsync(LiveActivity activity, CancellationToken cancellationToken = default)
        => EnqueueAsync(new LiveRequest { Activity = activity }, cancellationToken);

    public void Close()
        => _channel.Writer.TryComplete();

    public async IAsyncEnumerable<LiveRequest> ReadAllAsync([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        while (await _channel.Reader.WaitToReadAsync(cancellationToken))
        {
            while (_channel.Reader.TryRead(out var item))
                yield return item;
        }
    }
}
