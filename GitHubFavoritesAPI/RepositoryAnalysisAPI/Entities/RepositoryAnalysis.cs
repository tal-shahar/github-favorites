namespace RepositoryAnalysisAPI.Entities;

public sealed class RepositoryAnalysis
{
    public Guid Id { get; set; }
    public Guid FavoriteId { get; set; }
    public string License { get; set; } = string.Empty;
    public List<string> Topics { get; set; } = new();
    public Dictionary<string, long> Languages { get; set; } = new();
    public string PrimaryLanguage { get; set; } = string.Empty;
    public int ReadmeLength { get; set; }
    public int OpenIssues { get; set; }
    public int Forks { get; set; }
    public int StarsSnapshot { get; set; }
    public int ActivityDays { get; set; }
    public string DefaultBranch { get; set; } = string.Empty;
    public double HealthScore { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public Favorite? Favorite { get; set; }
}

