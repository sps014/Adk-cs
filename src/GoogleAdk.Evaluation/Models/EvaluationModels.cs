using GoogleAdk.Core.Abstractions.Events;
using GoogleAdk.Core.Abstractions.Models;

namespace GoogleAdk.Evaluation.Models;

public sealed class EvalSet
{
    public string EvalSetId { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Description { get; set; }
    public List<EvalCase> EvalCases { get; set; } = new();
    public DateTimeOffset? CreationTimestamp { get; set; }
}

public sealed class EvalCase
{
    public string EvalId { get; set; } = string.Empty;
    public List<Invocation> Conversation { get; set; } = new();
    public Dictionary<string, object?>? SessionInput { get; set; }
    public Dictionary<string, object?>? FinalSessionState { get; set; }
}

public sealed class Invocation
{
    public Content? UserContent { get; set; }
    public Content? FinalResponse { get; set; }
    public IntermediateData? IntermediateData { get; set; }
}

public sealed class IntermediateData
{
    public List<Event>? ToolUses { get; set; }
    public List<Event>? ToolResponses { get; set; }
    public List<Event>? IntermediateResponses { get; set; }
}

public sealed class EvalCaseResult
{
    public string EvalId { get; set; } = string.Empty;
    public List<InvocationResult> Invocations { get; set; } = new();
    public Dictionary<string, object?>? FinalSessionState { get; set; }
}

public sealed class InvocationResult
{
    public Content? FinalResponse { get; set; }
    public Dictionary<string, EvalMetricResult> Metrics { get; set; } = new();
}

public sealed class EvalMetric
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public sealed class EvalMetricResult
{
    public string MetricName { get; set; } = string.Empty;
    public double? Score { get; set; }
    public string? Reason { get; set; }
}
