namespace RepositoryAnalysisAPI.Models;

public sealed class AnalysisResponse
{
    public Guid FavoriteId { get; init; }
    public string RepoId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Owner { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int Stars { get; init; }
    public DateTime UpdatedAtUtc { get; init; }
    public DateTime AnalyzedAtUtc { get; init; }
    public string License { get; init; } = string.Empty;
    public IReadOnlyCollection<string> Topics { get; init; } = Array.Empty<string>();
    public IReadOnlyDictionary<string, long> Languages { get; init; } = new Dictionary<string, long>();
    public string PrimaryLanguage { get; init; } = string.Empty;
    public int ReadmeLength { get; init; }
    public int OpenIssues { get; init; }
    public int Forks { get; init; }
    public int StarsSnapshot { get; init; }
    public int ActivityDays { get; init; }
    public string DefaultBranch { get; init; } = string.Empty;
    public double HealthScore { get; init; }
}

