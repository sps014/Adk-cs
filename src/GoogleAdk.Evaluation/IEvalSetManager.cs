using GoogleAdk.Evaluation.Models;

namespace GoogleAdk.Evaluation;

/// <summary>
/// Interface for managing and persisting evaluation sets and their execution results.
/// </summary>
public interface IEvalSetManager
{
    /// <summary>
    /// Saves an evaluation set.
    /// </summary>
    Task SaveEvalSetAsync(EvalSet evalSet, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an evaluation set by ID.
    /// </summary>
    Task<EvalSet?> GetEvalSetAsync(string evalSetId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves the results of an evaluation run.
    /// </summary>
    Task SaveEvaluationRunAsync(string runId, string evalSetId, List<EvalCaseResult> results, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the results of an evaluation run.
    /// </summary>
    Task<List<EvalCaseResult>?> GetEvaluationRunAsync(string runId, CancellationToken cancellationToken = default);
}
