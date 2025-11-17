namespace FavoritesAPI.Models.Favorites;

public sealed class AnalysisDto
{
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

