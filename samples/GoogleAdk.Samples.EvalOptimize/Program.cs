// ============================================================================
// Evaluation + Optimization Sample
// ============================================================================

using GoogleAdk.Evaluation;
using GoogleAdk.Evaluation.Models;
using GoogleAdk.Optimization;

Console.WriteLine("=== Eval + Optimize Sample ===\n");

var evalSet = new EvalSet
{
    EvalSetId = "sample-set",
    EvalCases = []
};

var evalService = new LocalEvalService();
var evalResults = await evalService.EvaluateAsync(evalSet, [], [], CancellationToken.None);
Console.WriteLine($"Eval results: {evalResults.Count}");

var optimizer = new SimplePromptOptimizer();
var optResult = await optimizer.OptimizeAsync("Initial prompt", new DummySampler());
Console.WriteLine($"Optimized prompt: {optResult.Optimized}");

Console.WriteLine("\n=== Eval + Optimize Sample Complete ===");

file sealed class DummySampler : ISampler<string>
{
    public Task<SamplingResult> SampleAndScoreAsync(string candidate, CancellationToken cancellationToken = default)
        => Task.FromResult(new SamplingResult { Score = 1.0 });
}
