namespace RepositoryAnalysisWorker.Messaging.Events;

public sealed class FavoriteFavoritedEvent
{
    public Guid FavoriteId { get; init; }
    public Guid UserId { get; init; }
    public string RepoId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Owner { get; init; } = string.Empty;
    public int Stars { get; init; }
    public DateTime RepoUpdatedAtUtc { get; init; }
}

