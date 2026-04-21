// ============================================================================
// Evaluation + Optimization Sample (LLM-powered)
// ============================================================================

using System.Text.RegularExpressions;
using GoogleAdk.Core;
using GoogleAdk.Core.Abstractions.Models;
using GoogleAdk.Core.Agents;
using GoogleAdk.Core.Runner;
using GoogleAdk.Evaluation;
using GoogleAdk.Evaluation.EfCore;
using GoogleAdk.Evaluation.Evaluators;
using GoogleAdk.Evaluation.Models;
using GoogleAdk.Optimization;
using Microsoft.EntityFrameworkCore;

AdkEnv.Load();

Console.WriteLine("=== Eval + Optimize Sample (LLM) ===\n");

var model = "gemini-2.5-flash";

// ---- 0. Set up EF Core for Evaluation Management --------------------------

var dbOptions = new DbContextOptionsBuilder<AdkEvaluationDbContext>()
    .UseInMemoryDatabase("EvalSampleDb")
    .Options;

var dbContext = new AdkEvaluationDbContext(dbOptions);
var evalSetManager = new EfCoreEvalSetManager(dbContext);

// ---- 1. Create and Save an eval set ---------------------------------------

var evalSet = new EvalSet
{
    EvalSetId = "sample-set",
    Name = "IT Summarization Set",
    Description = "Checks the ability to summarize complex IT concepts.",
    EvalCases =
    [
        new EvalCase
        {
            EvalId = "gdpr",
            Conversation =
            [
                new Invocation
                {
                    UserContent = new Content
                    {
                        Role = "user",
                        Parts = [new Part { Text = "In one sentence, summarize GDPR." }]
                    },
                    FinalResponse = new Content
                    {
                        Role = "model",
                        Parts = [new Part { Text = "GDPR is a privacy and security law that imposes obligations onto organizations anywhere, so long as they target or collect data related to people in the EU." }]
                    }
                }
            ]
        },
        new EvalCase
        {
            EvalId = "slo",
            Conversation =
            [
                new Invocation
                {
                    UserContent = new Content
                    {
                        Role = "user",
                        Parts = [new Part { Text = "Explain SLOs to a new engineer in one sentence." }]
                    },
                    FinalResponse = new Content
                    {
                        Role = "model",
                        Parts = [new Part { Text = "An SLO is an agreement within an SLA about a specific metric like uptime or response time." }]
                    }
                }
            ]
        }
    ]
};

await evalSetManager.SaveEvalSetAsync(evalSet);
Console.WriteLine($"Saved EvalSet {evalSet.EvalSetId}");

// ---- 2. Run inference for the eval set ------------------------------------

var writerAgent = new LlmAgent(new LlmAgentConfig
{
    Name = "writer",
    Model = model,
    Instruction = "Answer in one concise sentence. Make it easy to understand for beginners."
});

var writerRunner = new InMemoryRunner("eval-opt", writerAgent);

var evalService = new LocalEvalService();
var inferenceResults = await evalService.PerformInferenceAsync(writerRunner, evalSet);

Console.WriteLine($"Inference results: {inferenceResults.Count} cases");

// ---- 3. Standard Evaluators (Rubric & Hallucination) ----------------------

var judgeModel = (LlmModel)model;

var rubricEvaluator = new RubricBasedEvaluator(
    name: "ClarityScore",
    judgeModel: judgeModel,
    rubricDescription: "Score 1.0 if the ACTUAL OUTPUT is highly clear and uses simple language. Score 0.5 if it uses jargon. Score 0.0 if it is incomprehensible."
);

var hallucinationEvaluator = new HallucinationEvaluator(judgeModel);

var scoredResults = await evalService.EvaluateAsync(
    evalSet, 
    inferenceResults, 
    [rubricEvaluator, hallucinationEvaluator]);

foreach (var caseResult in scoredResults)
{
    Console.WriteLine($"Case: {caseResult.EvalId}");
    foreach (var metricKvp in caseResult.Invocations[0].Metrics)
    {
        var metric = metricKvp.Value;
        Console.WriteLine($"  - {metric.MetricName}: {metric.Score:0.00} ({metric.Reason})");
    }
}

// Save the evaluation run to EF Core
var runId = Guid.NewGuid().ToString("N");
await evalSetManager.SaveEvaluationRunAsync(runId, evalSet.EvalSetId, scoredResults);
Console.WriteLine($"\nSaved Evaluation Run {runId}");

// ---- 4. LLM-based prompt optimization -------------------------------------

var judgeRunner = CreateJudgeRunner(model);
var optimizer = new SimplePromptOptimizer();
var sampler = new LlmPromptSampler(
    judgeRunner,
    "Write a concise, friendly refund reply that confirms the timeline.");

var optResult = await optimizer.OptimizeAsync(
    "Write a reply to the customer about their refund.",
    sampler);

Console.WriteLine($"\nOptimized prompt: {optResult.Optimized}");
Console.WriteLine($"Candidate count: {optResult.Candidates.Count}");

Console.WriteLine("\n=== Eval + Optimize Sample Complete ===");

static Runner CreateJudgeRunner(string model)
{
    var judgeAgent = new LlmAgent(new LlmAgentConfig
    {
        Name = "judge",
        Model = model,
        Instruction = "You are a strict grader. Output only a number between 0 and 1."
    });

    return new InMemoryRunner("eval-opt-judge", judgeAgent);
}

file sealed class LlmPromptSampler : ISampler<string>
{
    private readonly Runner _runner;
    private readonly string _goal;

    public LlmPromptSampler(Runner runner, string goal)
    {
        _runner = runner;
        _goal = goal;
    }

    public async Task<SamplingResult> SampleAndScoreAsync(
        string candidate,
        CancellationToken cancellationToken = default)
    {
        var prompt = $"""
Score how well this prompt helps an assistant achieve the goal.
Goal: {_goal}
Prompt: {candidate}
Return only a number between 0 and 1.
""";

        var (score, raw) = await EvalHelpers.ScoreWithLlmAsync(_runner, prompt, cancellationToken);
        return new SamplingResult
        {
            Score = score,
            Data = new Dictionary<string, object?> { ["raw"] = raw }
        };
    }
}

static class EvalHelpers
{
    public static async Task<(double score, string raw)> ScoreWithLlmAsync(
        Runner runner,
        string prompt,
        CancellationToken cancellationToken)
    {
        var content = new Content
        {
            Role = "user",
            Parts = [new Part { Text = prompt }]
        };

        string raw = "0";
        await foreach (var evt in runner.RunEphemeralAsync("judge", content, cancellationToken: cancellationToken))
        {
            if (evt.IsFinalResponse())
                raw = evt.StringifyContent().Trim();
        }

        return (ParseScore(raw), raw);
    }

    private static double ParseScore(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;

        var match = Regex.Match(text, @"\d+(\.\d+)?");
        if (!match.Success) return 0;
        if (!double.TryParse(match.Value, out var score)) return 0;

        return Math.Clamp(score, 0, 1);
    }
}


