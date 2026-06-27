using System.Runtime.CompilerServices;
using GoogleAdk.Core.Abstractions.Events;
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Core.Agents;

namespace GoogleAdk.E2e.Tests;

/// <summary>
/// A test double <see cref="BaseLlm"/> that records the most recent
/// <see cref="LlmRequest"/> and always replies with a fixed "ok" message.
/// Shared across the E2E tests that need to assert on what the agent sent to
/// the model without making a real network call.
/// </summary>
internal sealed class CapturingLlm : BaseLlm
{
    /// <summary>The last request the agent passed to the model.</summary>
    public LlmRequest? LastRequest { get; private set; }

    public CapturingLlm(string model) : base(model) { }

    public override async IAsyncEnumerable<LlmResponse> GenerateContentAsync(
        LlmRequest llmRequest,
        bool stream = false,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        LastRequest = llmRequest;
        yield return new LlmResponse
        {
            Content = new Content { Role = "model", Parts = [new Part { Text = "ok" }] }
        };
        await Task.CompletedTask;
    }

    public override Task<BaseLlmConnection> ConnectAsync(LlmRequest llmRequest)
        => Task.FromResult<BaseLlmConnection>(new StreamingLlmConnection(this, llmRequest));
}
