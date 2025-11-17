namespace RepositoryAnalysisWorker.Entities;

public sealed class Favorite
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string RepoId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Stars { get; set; }
    public DateTime RepoUpdatedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public RepositoryAnalysis? Analysis { get; set; }
}

