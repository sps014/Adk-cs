using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Evaluation.Models;

namespace GoogleAdk.Evaluation.Evaluators;

/// <summary>
/// Evaluates whether the actual output contains hallucinations (claims not supported by the context/expected output).
/// Score of 1.0 means NO hallucination (perfectly grounded). Score of 0.0 means complete hallucination.
/// </summary>
public sealed class HallucinationEvaluator : IEvalMetricEvaluator
{
    private readonly RubricBasedEvaluator _innerEvaluator;

    public string Name => _innerEvaluator.Name;

    public HallucinationEvaluator(LlmModel judgeModel, string name = "Hallucination") 
    {
        var rubric = @"
Evaluate if the ACTUAL OUTPUT contains any factual claims that are NOT supported by the EXPECTED OUTPUT (which serves as the source of truth/context).
- Score 1.0: All factual claims in the ACTUAL OUTPUT are fully supported by the EXPECTED OUTPUT.
- Score 0.5: Some factual claims are supported, but there are minor unsupported claims (hallucinations).
- Score 0.0: The ACTUAL OUTPUT contains significant factual claims that contradict or are completely absent from the EXPECTED OUTPUT.

If EXPECTED OUTPUT is empty or <No Expected Output>, evaluate if the ACTUAL OUTPUT contains universally known falsehoods or logical contradictions based on the USER INPUT.
";
        _innerEvaluator = new RubricBasedEvaluator(name, judgeModel, rubric, useJsonSchema: true);
    }

    public Task<EvalMetricResult> EvaluateAsync(Invocation invocation, InvocationResult result, CancellationToken cancellationToken = default)
    {
        return _innerEvaluator.EvaluateAsync(invocation, result, cancellationToken);
    }
}
