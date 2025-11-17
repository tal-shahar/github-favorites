namespace RepositoryAnalysisWorker.Models;

public sealed class RepositoryMetadata
{
    public string License { get; init; } = string.Empty;
    public List<string> Topics { get; init; } = new();
    public Dictionary<string, long> Languages { get; init; } = new();
    public string PrimaryLanguage { get; init; } = string.Empty;
    public int ReadmeLength { get; init; }
    public int OpenIssues { get; init; }
    public int Forks { get; init; }
    public int StarsSnapshot { get; init; }
    public int ActivityDays { get; init; }
    public string DefaultBranch { get; init; } = string.Empty;
    public DateTime RetrievedAtUtc { get; init; } = DateTime.UtcNow;
}

