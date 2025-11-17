using System.ComponentModel.DataAnnotations;

namespace FavoritesAPI.Models.Search;

public sealed class SearchQuery
{
    [Required]
    public string Q { get; init; } = string.Empty;

    [Range(1, 10)]
    public int Page { get; init; } = 1;

    [Range(1, 50)]
    public int PerPage { get; init; } = 10;
}

