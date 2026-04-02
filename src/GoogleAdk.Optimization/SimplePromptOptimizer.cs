namespace GoogleAdk.Optimization;

/// <summary>
/// Simple optimizer that can iterate on a prompt candidate.
/// </summary>
public sealed class SimplePromptOptimizer : IOptimizer<string>
{
    public Task<OptimizerResult<string>> OptimizeAsync(
        string initial,
        ISampler<string> sampler,
        CancellationToken cancellationToken = default)
    {
        var result = new OptimizerResult<string>
        {
            Optimized = initial,
            Candidates = new List<string> { initial }
        };
        return Task.FromResult(result);
    }
}
