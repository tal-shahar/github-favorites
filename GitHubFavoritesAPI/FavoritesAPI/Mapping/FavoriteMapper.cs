using FavoritesAPI.Entities;
using FavoritesAPI.Models.Favorites;

namespace FavoritesAPI.Mapping;

internal static class FavoriteMapper
{
    public static FavoriteResponse ToResponse(this Favorite favorite)
    {
        return new FavoriteResponse
        {
            Id = favorite.Id,
            RepoId = favorite.RepoId,
            Name = favorite.Name,
            Owner = favorite.Owner,
            Description = favorite.Description,
            Stars = favorite.Stars,
            UpdatedAtUtc = favorite.RepoUpdatedAtUtc,
            CreatedAtUtc = favorite.CreatedAtUtc,
            Analysis = favorite.Analysis is null
                ? null
                : new AnalysisDto
                {
                    License = favorite.Analysis.License,
                    Topics = favorite.Analysis.Topics.AsReadOnly(),
                    Languages = favorite.Analysis.Languages,
                    PrimaryLanguage = favorite.Analysis.PrimaryLanguage,
                    ReadmeLength = favorite.Analysis.ReadmeLength,
                    OpenIssues = favorite.Analysis.OpenIssues,
                    Forks = favorite.Analysis.Forks,
                    StarsSnapshot = favorite.Analysis.StarsSnapshot,
                    ActivityDays = favorite.Analysis.ActivityDays,
                    DefaultBranch = favorite.Analysis.DefaultBranch,
                    HealthScore = Math.Round(favorite.Analysis.HealthScore, 2)
                }
        };
    }
}

