namespace GoogleAdk.Optimization;

public sealed class SamplingResult
{
    public double Score { get; set; }
    public Dictionary<string, object?>? Data { get; set; }
}

public sealed class OptimizerResult<T>
{
    public T? Optimized { get; set; }
    public List<T> Candidates { get; set; } = new();
}

public interface ISampler<TCandidate>
{
    Task<SamplingResult> SampleAndScoreAsync(TCandidate candidate, CancellationToken cancellationToken = default);
}

public interface IOptimizer<TCandidate>
{
    Task<OptimizerResult<TCandidate>> OptimizeAsync(TCandidate initial, ISampler<TCandidate> sampler, CancellationToken cancellationToken = default);
}
