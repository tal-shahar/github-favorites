namespace FavoritesAPI.Models.Search;

public sealed class RepositorySearchResult
{
    public string RepoId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Owner { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int Stars { get; init; }
    public DateTime UpdatedAtUtc { get; init; }
}

