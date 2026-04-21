# LlmAsJudgeEvaluator

The `LlmAsJudgeEvaluator` is the abstract base class powering all LLM-driven evaluation tools in the `GoogleAdk.Evaluation` library. It provides the core boilerplate needed to use a language model as a grader for another agent's output.

## Overview

Instead of hardcoding deterministic checks (like string matching or regex), an LLM-as-a-judge allows you to grade subjective quality, adherence to instructions, and semantic meaning. 

The ADK provides concrete implementations like [`RubricBasedEvaluator`](rubric-based-evaluator.md) and [`HallucinationEvaluator`](hallucination-evaluator.md), but you can build your own custom evaluators by inheriting from `LlmAsJudgeEvaluator`.

## Creating a Custom LLM Evaluator

To create a custom evaluator, inherit from `LlmAsJudgeEvaluator` and implement two required methods:

1. `FormatAutoRaterPrompt`: Defines what text is sent to the judge model.
2. `ConvertAutoRaterResponseToScoreAsync`: Parses the judge's response back into a numeric score and reasoning string.

### Example: A Word Count Evaluator

While word count could be evaluated deterministically, this example demonstrates the implementation pattern for an LLM judge:

```csharp
using GoogleAdk.Core.Abstractions.Events;
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Evaluation.Evaluators;
using GoogleAdk.Evaluation.Models;

public class BrevityEvaluator : LlmAsJudgeEvaluator
{
    public BrevityEvaluator(LlmModel judgeModel) 
        : base("Brevity", judgeModel) { }

    protected override string FormatAutoRaterPrompt(Invocation invocation, InvocationResult result)
    {
        var actualText = result.FinalResponse?.Parts?.FirstOrDefault()?.Text ?? "";

        return $@"
Evaluate the following text for brevity.
If the text is under 15 words, give it a 1.0.
If it is between 15 and 30 words, give it a 0.5.
If it is over 30 words, give it a 0.0.

TEXT:
{actualText}

Return your answer strictly in the format: SCORE|REASONING
";
    }

    protected override Task<EvalMetricResult> ConvertAutoRaterResponseToScoreAsync(
        LlmResponse autoRaterResponse, 
        CancellationToken cancellationToken)
    {
        var text = autoRaterResponse.Content?.Parts?.FirstOrDefault()?.Text ?? "";
        
        var parts = text.Split('|');
        if (parts.Length == 2 && double.TryParse(parts[0], out double score))
        {
            return Task.FromResult(new EvalMetricResult
            {
                MetricName = Name,
                Score = score,
                Reason = parts[1].Trim()
            });
        }

        return Task.FromResult(new EvalMetricResult
        {
            MetricName = Name,
            Score = null,
            Reason = "Failed to parse judge output: " + text
        });
    }
}
```

### Using Your Custom Evaluator

Once created, you can pass your custom evaluator into the evaluation service just like the built-in ones.

```csharp
var brevityEvaluator = new BrevityEvaluator("gemini-2.5-flash");

var scoredResults = await evalService.EvaluateAsync(
    evalSet, 
    inferenceResults, 
    [brevityEvaluator]);
```