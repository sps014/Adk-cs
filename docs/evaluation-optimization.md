# Evaluation & Optimization

The ADK includes dedicated libraries (`GoogleAdk.Evaluation`, `GoogleAdk.Evaluation.EfCore`, and `GoogleAdk.Optimization`) to systematically measure and improve the quality of your LLM prompts and agent responses.

## 1. Running Evaluations (LLM-as-a-Judge)

Instead of relying on manual testing, you can use a separate LLM agent (the "Judge") to mathematically grade the responses of your core agent based on defined criteria.

### Setting up the Evaluation Set and Manager

An `EvalSet` defines a collection of test cases with simulated user conversations. The ADK includes `EfCoreEvalSetManager` to seamlessly persist your evaluation sets and runs using Entity Framework Core.

```csharp
using GoogleAdk.Evaluation;
using GoogleAdk.Evaluation.EfCore;
using GoogleAdk.Evaluation.Models;
using Microsoft.EntityFrameworkCore;

// Set up the EF Core context and manager
var dbOptions = new DbContextOptionsBuilder<AdkEvaluationDbContext>()
    .UseInMemoryDatabase("EvalSampleDb") // Use SQL Server, PostgreSQL, etc. in production
    .Options;

var dbContext = new AdkEvaluationDbContext(dbOptions);
var evalSetManager = new EfCoreEvalSetManager(dbContext);

var evalSet = new EvalSet
{
    EvalSetId = "summarization-test",
    Name = "IT Summarization Set",
    EvalCases =
    [
        new EvalCase
        {
            EvalId = "case_gdpr",
            Conversation =
            [
                new Invocation 
                { 
                    UserContent = new Content { Role = "user", Parts = [new Part { Text = "Summarize GDPR." }] },
                    FinalResponse = new Content { Role = "model", Parts = [new Part { Text = "GDPR is a privacy and security law..." }] }
                }
            ]
        }
    ]
};

await evalSetManager.SaveEvalSetAsync(evalSet);
```

### Performing Inference

Generate the initial responses from your core agent over the evaluation dataset.

```csharp
var evalService = new LocalEvalService();

// Run the core agent against the dataset
var inferenceResults = await evalService.PerformInferenceAsync(myCoreRunner, evalSet);
```

### Scoring with Built-in Evaluators

You can use the built-in evaluators to grade the responses. The ADK provides evaluators like `RubricBasedEvaluator` and `HallucinationEvaluator`.

```csharp
using GoogleAdk.Evaluation.Evaluators;

var judgeModel = new LlmModel("gemini-2.5-flash");

// 1. Rubric-based Evaluator
var rubricEvaluator = new RubricBasedEvaluator(
    name: "ClarityScore",
    judgeModel: judgeModel,
    rubricDescription: "Score 1.0 if the ACTUAL OUTPUT is highly clear and uses simple language. Score 0.5 if it uses jargon. Score 0.0 if it is incomprehensible."
);

// 2. Hallucination Evaluator
var hallucinationEvaluator = new HallucinationEvaluator(judgeModel);

// Evaluate the original inference responses using the Judge
var scoredResults = await evalService.EvaluateAsync(
    evalSet, 
    inferenceResults, 
    [rubricEvaluator, hallucinationEvaluator]);

foreach (var result in scoredResults)
{
    Console.WriteLine($"Case: {result.EvalId}");
    foreach (var metricKvp in result.Invocations[0].Metrics)
    {
        var metric = metricKvp.Value;
        Console.WriteLine($"  - {metric.MetricName}: {metric.Score:0.00} ({metric.Reason})");
    }
}

// Save the evaluation run results
await evalSetManager.SaveEvaluationRunAsync(Guid.NewGuid().ToString("N"), evalSet.EvalSetId, scoredResults);
```

For more details on specific evaluators, see:
- [RubricBasedEvaluator](rubric-based-evaluator.md)
- [HallucinationEvaluator](hallucination-evaluator.md)
- [LlmAsJudgeEvaluator](llm-as-judge-evaluator.md)

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