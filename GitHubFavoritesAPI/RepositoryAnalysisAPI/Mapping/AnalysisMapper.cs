using RepositoryAnalysisAPI.Entities;
using RepositoryAnalysisAPI.Models;

namespace RepositoryAnalysisAPI.Mapping;

internal static class AnalysisMapper
{
    public static AnalysisResponse ToResponse(this RepositoryAnalysis analysis)
    {
        if (analysis.Favorite is null)
        {
            throw new InvalidOperationException("Analysis requires favorite data");
        }

        return new AnalysisResponse
        {
            FavoriteId = analysis.FavoriteId,
            RepoId = analysis.Favorite.RepoId,
            Name = analysis.Favorite.Name,
            Owner = analysis.Favorite.Owner,
            Description = analysis.Favorite.Description,
            Stars = analysis.Favorite.Stars,
            UpdatedAtUtc = analysis.Favorite.RepoUpdatedAtUtc,
            AnalyzedAtUtc = analysis.CreatedAtUtc,
            License = analysis.License,
            Topics = analysis.Topics.AsReadOnly(),
            Languages = analysis.Languages,
            PrimaryLanguage = analysis.PrimaryLanguage,
            ReadmeLength = analysis.ReadmeLength,
            OpenIssues = analysis.OpenIssues,
            Forks = analysis.Forks,
            StarsSnapshot = analysis.StarsSnapshot,
            ActivityDays = analysis.ActivityDays,
            DefaultBranch = analysis.DefaultBranch,
            HealthScore = Math.Round(analysis.HealthScore, 2)
        };
    }
}

