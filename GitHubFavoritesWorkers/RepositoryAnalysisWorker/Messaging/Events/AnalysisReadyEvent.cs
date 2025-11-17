namespace RepositoryAnalysisWorker.Messaging.Events;

public sealed class AnalysisReadyEvent
{
    public Guid FavoriteId { get; init; }
    public string RepoId { get; init; } = string.Empty;
    public Guid UserId { get; init; }
    public double HealthScore { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}

