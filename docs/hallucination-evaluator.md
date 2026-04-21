# HallucinationEvaluator

The `HallucinationEvaluator` is a built-in evaluation tool in the `GoogleAdk.Evaluation` library. It specifically measures whether an agent's response contains factual claims that are unsupported by the provided context or expected output.

## Overview

Hallucinations are one of the most critical risks when deploying LLMs. This evaluator acts as an LLM Judge, comparing the agent's output against the ground truth (the `Expected Output`) to ensure no extraneous or conflicting information was generated.

A score of `1.0` means the response is perfectly grounded. A score of `0.0` means it contains severe hallucinations.

## Usage

You only need to supply the LLM judge model. The grading rubric for hallucination detection is pre-configured internally.

```csharp
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Evaluation.Evaluators;

// It is generally recommended to use your smartest model (e.g., gemini-2.5-pro) for complex evaluations like hallucination checking.
var judgeModel = new LlmModel("gemini-2.5-pro");

var hallucinationEvaluator = new HallucinationEvaluator(judgeModel, name: "HallucinationMetric");
```

### Applying the Evaluator

You pass the `HallucinationEvaluator` to `LocalEvalService.EvaluateAsync`, typically alongside other evaluators.

```csharp
var scoredResults = await evalService.EvaluateAsync(
    evalSet, 
    inferenceResults, 
    [hallucinationEvaluator]);

var metric = scoredResults[0].Invocations[0].Metrics["HallucinationMetric"];

if (metric.Score < 1.0) 
{
    Console.WriteLine("Warning: Hallucination detected!");
    Console.WriteLine($"Judge's Reasoning: {metric.Reason}");
}
```

## How It Works

The `HallucinationEvaluator` internally wraps the `RubricBasedEvaluator` using a strict rubric designed specifically for factual consistency. 

When evaluating an invocation, it behaves according to these rules:
1. **With Expected Output:** It treats the `EvalCase`'s `FinalResponse` as the definitive source of truth. If the agent's actual output makes factual claims not found in the expected output, the score is reduced.
2. **Without Expected Output:** If your `EvalCase` does not include an expected output, it falls back to checking if the agent's actual output contains universally known falsehoods or logical contradictions based on the user's input.