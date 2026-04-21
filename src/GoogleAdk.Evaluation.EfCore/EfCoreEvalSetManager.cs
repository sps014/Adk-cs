using System.Text.Json;
using GoogleAdk.Evaluation.Models;
using Microsoft.EntityFrameworkCore;

namespace GoogleAdk.Evaluation.EfCore;

/// <summary>
/// EF Core implementation of IEvalSetManager.
/// </summary>
public class EfCoreEvalSetManager : IEvalSetManager
{
    private readonly AdkEvaluationDbContext _dbContext;
    private readonly JsonSerializerOptions _jsonOptions;

    public EfCoreEvalSetManager(AdkEvaluationDbContext dbContext)
    {
        _dbContext = dbContext;
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    public async Task SaveEvalSetAsync(EvalSet evalSet, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.EvalSets.FindAsync(new object[] { evalSet.EvalSetId }, cancellationToken);
        var json = JsonSerializer.Serialize(evalSet.EvalCases, _jsonOptions);

        if (existing == null)
        {
            _dbContext.EvalSets.Add(new StorageEvalSet
            {
                Id = evalSet.EvalSetId,
                Name = evalSet.Name,
                Description = evalSet.Description,
                CreationTimestamp = evalSet.CreationTimestamp ?? DateTimeOffset.UtcNow,
                EvalCasesJson = json
            });
        }
        else
        {
            existing.Name = evalSet.Name;
            existing.Description = evalSet.Description;
            existing.EvalCasesJson = json;
            _dbContext.EvalSets.Update(existing);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<EvalSet?> GetEvalSetAsync(string evalSetId, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.EvalSets.FindAsync(new object[] { evalSetId }, cancellationToken);
        if (entity == null) return null;

        var evalCases = JsonSerializer.Deserialize<List<EvalCase>>(entity.EvalCasesJson, _jsonOptions) ?? new List<EvalCase>();

        return new EvalSet
        {
            EvalSetId = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            CreationTimestamp = entity.CreationTimestamp,
            EvalCases = evalCases
        };
    }

    public async Task SaveEvaluationRunAsync(string runId, string evalSetId, List<EvalCaseResult> results, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(results, _jsonOptions);

        var existing = await _dbContext.EvalRuns.FindAsync(new object[] { runId }, cancellationToken);
        if (existing == null)
        {
            _dbContext.EvalRuns.Add(new StorageEvalRun
            {
                RunId = runId,
                EvalSetId = evalSetId,
                ResultsJson = json,
                RunTimestamp = DateTimeOffset.UtcNow
            });
        }
        else
        {
            existing.EvalSetId = evalSetId;
            existing.ResultsJson = json;
            _dbContext.EvalRuns.Update(existing);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<EvalCaseResult>?> GetEvaluationRunAsync(string runId, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.EvalRuns.FindAsync(new object[] { runId }, cancellationToken);
        if (entity == null) return null;

        return JsonSerializer.Deserialize<List<EvalCaseResult>>(entity.ResultsJson, _jsonOptions);
    }
}
