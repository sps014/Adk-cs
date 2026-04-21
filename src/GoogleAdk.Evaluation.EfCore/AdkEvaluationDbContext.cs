using Microsoft.EntityFrameworkCore;

namespace GoogleAdk.Evaluation.EfCore;

/// <summary>
/// EF Core entity for storing an EvalSet.
/// </summary>
public class StorageEvalSet
{
    public string Id { get; set; } = null!;
    public string? Name { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset? CreationTimestamp { get; set; }
    
    /// <summary>
    /// Serialized list of EvalCases.
    /// </summary>
    public string EvalCasesJson { get; set; } = "[]";
}

/// <summary>
/// EF Core entity for storing an Evaluation Run Result.
/// </summary>
public class StorageEvalRun
{
    public string RunId { get; set; } = null!;
    public string EvalSetId { get; set; } = null!;
    public DateTimeOffset RunTimestamp { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Serialized list of EvalCaseResults.
    /// </summary>
    public string ResultsJson { get; set; } = "[]";
}

public class AdkEvaluationDbContext : DbContext
{
    public DbSet<StorageEvalSet> EvalSets => Set<StorageEvalSet>();
    public DbSet<StorageEvalRun> EvalRuns => Set<StorageEvalRun>();

    public AdkEvaluationDbContext(DbContextOptions<AdkEvaluationDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StorageEvalSet>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<StorageEvalRun>(entity =>
        {
            entity.HasKey(e => e.RunId);
            entity.HasIndex(e => e.EvalSetId);
        });
    }
}
