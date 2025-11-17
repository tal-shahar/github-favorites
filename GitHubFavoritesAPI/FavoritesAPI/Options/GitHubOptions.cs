namespace FavoritesAPI.Options;

public sealed class GitHubOptions
{
    public const string SectionName = "GitHub";

    public string ApiBaseUrl { get; set; } = "https://api.github.com";
    public string PersonalAccessToken { get; set; } = "ghp_placeholder_token_replace";
    public string UserAgent { get; set; } = "github-favorites-app";
}

