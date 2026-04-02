using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Runner;
using GoogleAdk.Evaluation.Models;

namespace GoogleAdk.Evaluation;

public interface IEvalService
{
    Task<List<EvalCaseResult>> PerformInferenceAsync(
        Runner runner,
        EvalSet evalSet,
        RunConfig? runConfig = null,
        CancellationToken cancellationToken = default);

    Task<List<EvalCaseResult>> EvaluateAsync(
        EvalSet evalSet,
        List<EvalCaseResult> inferenceResults,
        IEnumerable<IEvalMetricEvaluator> evaluators,
        CancellationToken cancellationToken = default);
}

public interface IEvalMetricEvaluator
{
    string Name { get; }
    Task<EvalMetricResult> EvaluateAsync(
        Invocation invocation,
        InvocationResult result,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Local evaluation service that runs agents against eval cases.
/// </summary>
public sealed class LocalEvalService : IEvalService
{
    public async Task<List<EvalCaseResult>> PerformInferenceAsync(
        Runner runner,
        EvalSet evalSet,
        RunConfig? runConfig = null,
        CancellationToken cancellationToken = default)
    {
        var results = new List<EvalCaseResult>();
        foreach (var evalCase in evalSet.EvalCases)
        {
            var caseResult = new EvalCaseResult { EvalId = evalCase.EvalId };
            foreach (var invocation in evalCase.Conversation)
            {
                var invocationResult = new InvocationResult();
                if (invocation.UserContent != null)
                {
                    GoogleAdk.Core.Abstractions.Events.Event? last = null;
                    await foreach (var evt in runner.RunEphemeralAsync(
                        userId: "eval",
                        newMessage: invocation.UserContent,
                        runConfig: runConfig,
                        cancellationToken: cancellationToken))
                    {
                        last = evt;
                    }

                    if (last?.Content != null)
                        invocationResult.FinalResponse = last.Content;
                }
                caseResult.Invocations.Add(invocationResult);
            }
            results.Add(caseResult);
        }

        return results;
    }

    public async Task<List<EvalCaseResult>> EvaluateAsync(
        EvalSet evalSet,
        List<EvalCaseResult> inferenceResults,
        IEnumerable<IEvalMetricEvaluator> evaluators,
        CancellationToken cancellationToken = default)
    {
        var evals = evaluators.ToList();
        foreach (var (evalCase, caseResult) in evalSet.EvalCases.Zip(inferenceResults))
        {
            for (var i = 0; i < evalCase.Conversation.Count; i++)
            {
                var invocation = evalCase.Conversation[i];
                var invocationResult = caseResult.Invocations[i];
                foreach (var evaluator in evals)
                {
                    var metricResult = await evaluator.EvaluateAsync(invocation, invocationResult, cancellationToken);
                    invocationResult.Metrics[evaluator.Name] = metricResult;
                }
            }
        }

        return inferenceResults;
    }
}
