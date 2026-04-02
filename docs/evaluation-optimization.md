# Evaluation & Optimization

The ADK includes dedicated libraries (`GoogleAdk.Evaluation` and `GoogleAdk.Optimization`) to systematically measure and improve the quality of your LLM prompts and agent responses.

## 1. Running Evaluations (LLM-as-a-Judge)

Instead of relying on manual testing, you can use a separate LLM agent (the "Judge") to mathematically grade the responses of your core agent based on defined criteria.

### Setting up the Evaluation Set

An `EvalSet` defines a collection of test cases with simulated user conversations.

```csharp
using GoogleAdk.Evaluation;
using GoogleAdk.Evaluation.Models;

var evalSet = new EvalSet
{
    EvalSetId = "summarization-test",
    EvalCases =
    [
        new EvalCase
        {
            EvalId = "case_gdpr",
            Conversation = [ new Invocation { UserContent = new Content { Role = "user", Parts = [new Part { Text = "Summarize GDPR." }] } } ]
        }
    ]
};
```

### Performing Inference

Generate the initial responses from your core agent over the evaluation dataset.

```csharp
var evalService = new LocalEvalService();

// Run the core agent against the dataset
var inferenceResults = await evalService.PerformInferenceAsync(myCoreRunner, evalSet);
```

### Scoring with a Judge Evaluator

You can define a custom evaluator that utilizes a strict LLM judge to grade the responses between 0.0 and 1.0.

```csharp
// The LLM-as-a-Judge evaluator configuration
var judgeAgent = new LlmAgent(new LlmAgentConfig
{
    Name = "judge",
    ModelName = "gemini-2.5-flash",
    Instruction = "You are a strict grader. Output ONLY a number between 0 and 1."
});
var judgeRunner = new InMemoryRunner("judge", judgeAgent);

var evaluator = new LlmJudgeEvaluator(judgeRunner);

// Evaluate the original inference responses using the Judge
var scoredResults = await evalService.EvaluateAsync(evalSet, inferenceResults, [evaluator]);

foreach (var result in scoredResults)
{
    var score = result.Invocations[0].Metrics[evaluator.Name].Score;
    Console.WriteLine($"Case {result.EvalId} scored: {score:0.00}");
}
```

## 2. Prompt Optimization

The `SimplePromptOptimizer` utilizes the LLM to automatically generate variations of a system prompt, evaluate them via a sampler, and output the mathematical best-performing candidate.

```csharp
using GoogleAdk.Optimization;

var optimizer = new SimplePromptOptimizer();

// The sampler simulates grading the effectiveness of the generated prompt candidates
var sampler = new LlmPromptSampler(judgeRunner, "Goal: Write a concise, friendly refund confirmation.");

var result = await optimizer.OptimizeAsync("Write a reply about a refund.", sampler);

Console.WriteLine($"Original Prompt: Write a reply about a refund.");
Console.WriteLine($"Optimized Prompt: {result.Optimized}");
```