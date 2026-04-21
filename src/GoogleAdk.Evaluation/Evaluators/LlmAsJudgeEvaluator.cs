using GoogleAdk.Core.Abstractions.Events;
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Evaluation.Models;

namespace GoogleAdk.Evaluation.Evaluators;

/// <summary>
/// Abstract base class for LLM-based evaluators.
/// It uses an LLM to generate an evaluation for a given invocation and its result.
/// </summary>
public abstract class LlmAsJudgeEvaluator : IEvalMetricEvaluator
{
    private readonly LlmModel _judgeModel;

    public string Name { get; }

    protected LlmAsJudgeEvaluator(string name, LlmModel judgeModel)
    {
        Name = name;
        _judgeModel = judgeModel;
    }

    /// <summary>
    /// Formats the prompt sent to the LLM judge.
    /// </summary>
    protected abstract string FormatAutoRaterPrompt(Invocation invocation, InvocationResult result);

    /// <summary>
    /// Parses the LLM judge's response and extracts the score/reasoning.
    /// </summary>
    protected abstract Task<EvalMetricResult> ConvertAutoRaterResponseToScoreAsync(
        LlmResponse autoRaterResponse, 
        CancellationToken cancellationToken);

    public async Task<EvalMetricResult> EvaluateAsync(
        Invocation invocation,
        InvocationResult result,
        CancellationToken cancellationToken = default)
    {
        var prompt = FormatAutoRaterPrompt(invocation, result);

        var llmRequest = new LlmRequest
        {
            Model = _judgeModel.ModelName,
            Contents = new List<Content>
            {
                new Content
                {
                    Role = "user",
                    Parts = new List<Part> { new Part { Text = prompt } }
                }
            }
        };

        var llm = _judgeModel.Resolve();
        LlmResponse? finalResponse = null;

        await foreach (var response in llm.GenerateContentAsync(llmRequest, stream: false, cancellationToken))
        {
            finalResponse = response;
        }

        if (finalResponse == null)
        {
            return new EvalMetricResult
            {
                MetricName = Name,
                Score = 0,
                Reason = "Judge model returned no response."
            };
        }

        return await ConvertAutoRaterResponseToScoreAsync(finalResponse, cancellationToken);
    }
}
