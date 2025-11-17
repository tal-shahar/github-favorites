namespace FavoritesAPI.Models.Favorites;

public sealed class FavoriteResponse
{
    public Guid Id { get; init; }
    public string RepoId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Owner { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int Stars { get; init; }
    public DateTime UpdatedAtUtc { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public AnalysisDto? Analysis { get; init; }
}

