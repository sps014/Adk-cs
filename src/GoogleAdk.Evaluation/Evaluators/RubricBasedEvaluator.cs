using System.Text.Json;
using System.Text.Json.Serialization;
using GoogleAdk.Core.Abstractions.Events;
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Evaluation.Models;

namespace GoogleAdk.Evaluation.Evaluators;

/// <summary>
/// A rubric-based evaluator that asks the LLM judge to score based on specific criteria.
/// </summary>
public sealed class RubricBasedEvaluator : LlmAsJudgeEvaluator
{
    private readonly string _rubricDescription;
    private readonly bool _useJsonSchema;

    public RubricBasedEvaluator(string name, LlmModel judgeModel, string rubricDescription, bool useJsonSchema = true) 
        : base(name, judgeModel)
    {
        _rubricDescription = rubricDescription;
        _useJsonSchema = useJsonSchema;
    }

    protected override string FormatAutoRaterPrompt(Invocation invocation, InvocationResult result)
    {
        var userText = invocation.UserContent?.Parts?.FirstOrDefault()?.Text ?? "<No User Input>";
        var expectedText = invocation.FinalResponse?.Parts?.FirstOrDefault()?.Text ?? "<No Expected Output>";
        var actualText = result.FinalResponse?.Parts?.FirstOrDefault()?.Text ?? "<No Actual Output>";

        var prompt = $@"
You are an expert evaluator. Evaluate the ACTUAL OUTPUT against the USER INPUT based on the given RUBRIC.
If EXPECTED OUTPUT is provided, you can use it as a reference for a perfect answer.

RUBRIC:
{_rubricDescription}

USER INPUT:
{userText}

EXPECTED OUTPUT:
{expectedText}

ACTUAL OUTPUT:
{actualText}
";

        if (_useJsonSchema)
        {
            prompt += @"
Please output a JSON object with exactly these fields:
- ""score"": A float between 0.0 and 1.0 representing the score.
- ""reason"": A string explaining the reasoning for the score.
";
        }

        return prompt;
    }

    protected override Task<EvalMetricResult> ConvertAutoRaterResponseToScoreAsync(LlmResponse autoRaterResponse, CancellationToken cancellationToken)
    {
        var text = autoRaterResponse.Content?.Parts?.FirstOrDefault()?.Text ?? string.Empty;
        
        try
        {
            // Try to extract JSON block if wrapped in markdown
            if (text.Contains("```json"))
            {
                var start = text.IndexOf("```json") + 7;
                var end = text.IndexOf("```", start);
                if (end > start)
                {
                    text = text.Substring(start, end - start);
                }
            }

            var parsed = JsonSerializer.Deserialize<AutoRaterResponseJson>(text, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (parsed != null)
            {
                return Task.FromResult(new EvalMetricResult
                {
                    MetricName = Name,
                    Score = parsed.Score,
                    Reason = parsed.Reason
                });
            }
        }
        catch
        {
            // Fallback parsing failed
        }

        return Task.FromResult(new EvalMetricResult
        {
            MetricName = Name,
            Score = null,
            Reason = "Failed to parse judge output. Raw output: " + text
        });
    }

    private class AutoRaterResponseJson
    {
        public double? Score { get; set; }
        public string? Reason { get; set; }
    }
}
