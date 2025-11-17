namespace RepositoryAnalysisWorker.Options;

public sealed class GitHubOptions
{
    public const string SectionName = "GitHub";

    public string ApiBaseUrl { get; set; } = "https://api.github.com";
    public string PersonalAccessToken { get; set; } = "ghp_replace_with_real_token";
    public string UserAgent { get; set; } = "github-favorites-worker";
}

