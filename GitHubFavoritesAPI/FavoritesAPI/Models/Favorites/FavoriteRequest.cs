using System.ComponentModel.DataAnnotations;

namespace FavoritesAPI.Models.Favorites;

public sealed class FavoriteRequest
{
    [Required]
    public string RepoId { get; init; } = string.Empty;

    [Required]
    public string Name { get; init; } = string.Empty;

    [Required]
    public string Owner { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int Stars { get; init; }

    public DateTime UpdatedAtUtc { get; init; }
}

