using System.Threading.Channels;
using GoogleAdk.Core.Abstractions.Events;

namespace GoogleAdk.Core.Abstractions.Models;

/// <summary>
/// Fallback live connection that uses GenerateContentAsync per user turn.
/// This provides a bidirectional-style interface even when the provider
/// does not support native live sessions.
/// </summary>
public sealed class StreamingLlmConnection : BaseLlmConnection
{
    private readonly BaseLlm _llm;
    private readonly LlmRequest _baseRequest;
    private readonly Channel<LlmResponse> _responses = Channel.CreateUnbounded<LlmResponse>();
    private readonly List<Content> _history = new();

    public StreamingLlmConnection(BaseLlm llm, LlmRequest baseRequest)
    {
        _llm = llm;
        _baseRequest = baseRequest;
        if (baseRequest.Contents.Count > 0)
            _history.AddRange(baseRequest.Contents);
    }

    public override Task SendHistoryAsync(IEnumerable<Content> history, CancellationToken cancellationToken = default)
    {
        _history.Clear();
        _history.AddRange(history);
        return Task.CompletedTask;
    }

    public override Task SendContentAsync(Content content, CancellationToken cancellationToken = default)
    {
        _history.Add(content);
        _ = Task.Run(async () =>
        {
            var request = new LlmRequest
            {
                Model = _baseRequest.Model,
                Config = _baseRequest.Config,
                ToolsDict = _baseRequest.ToolsDict,
                Contents = new List<Content>(_history),
            };

            await foreach (var response in _llm.GenerateContentAsync(request, stream: true, cancellationToken))
            {
                await _responses.Writer.WriteAsync(response, cancellationToken);
                if (response.TurnComplete == true && response.Content != null)
                {
                    _history.Add(response.Content);
                }
            }
        }, cancellationToken);

        return Task.CompletedTask;
    }

    public override Task SendRealtimeAsync(Part part, CancellationToken cancellationToken = default)
    {
        var content = new Content
        {
            Role = "user",
            Parts = new List<Part> { part },
        };
        return SendContentAsync(content, cancellationToken);
    }

    public override async IAsyncEnumerable<LlmResponse> ReceiveAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        while (await _responses.Reader.WaitToReadAsync(cancellationToken))
        {
            while (_responses.Reader.TryRead(out var item))
                yield return item;
        }
    }

    public override ValueTask DisposeAsync()
    {
        _responses.Writer.TryComplete();
        return ValueTask.CompletedTask;
    }
}
