using Microsoft.EntityFrameworkCore;
using RepositoryAnalysisWorker.Data;
using RepositoryAnalysisWorker.Entities;
using RepositoryAnalysisWorker.Models;
using RepositoryAnalysisWorker.Services.Contracts;

namespace RepositoryAnalysisWorker.Services;

public sealed class RepositoryAnalysisService(AppDbContext dbContext, ILogger<RepositoryAnalysisService> logger)
    : IRepositoryAnalysisService
{
    public async Task<RepositoryAnalysis> UpsertAnalysisAsync(Guid favoriteId, RepositoryMetadata metadata, CancellationToken cancellationToken)
    {
        var favorite = await dbContext.Favorites.SingleOrDefaultAsync(f => f.Id == favoriteId, cancellationToken);
        if (favorite is null)
        {
            throw new InvalidOperationException($"Favorite {favoriteId} not found.");
        }

        var analysis = await dbContext.RepositoryAnalyses
            .SingleOrDefaultAsync(a => a.FavoriteId == favoriteId, cancellationToken);

        if (analysis is null)
        {
            analysis = new RepositoryAnalysis
            {
                FavoriteId = favoriteId
            };
            dbContext.RepositoryAnalyses.Add(analysis);
        }

        analysis.License = metadata.License;
        analysis.Topics = metadata.Topics;
        analysis.Languages = metadata.Languages;
        analysis.PrimaryLanguage = metadata.PrimaryLanguage;
        analysis.ReadmeLength = metadata.ReadmeLength;
        analysis.OpenIssues = metadata.OpenIssues;
        analysis.Forks = metadata.Forks;
        analysis.StarsSnapshot = metadata.StarsSnapshot;
        analysis.ActivityDays = metadata.ActivityDays;
        analysis.DefaultBranch = metadata.DefaultBranch;
        analysis.HealthScore = HealthScoreCalculator.Calculate(
            metadata.StarsSnapshot,
            metadata.ActivityDays,
            metadata.Forks,
            metadata.OpenIssues,
            metadata.ReadmeLength);
        analysis.CreatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Analysis stored for favorite {FavoriteId}", favoriteId);

        return analysis;
    }
}

