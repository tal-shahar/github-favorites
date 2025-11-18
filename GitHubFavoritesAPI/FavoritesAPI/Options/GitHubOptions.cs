namespace FavoritesAPI.Options;

public sealed class GitHubOptions
{
    public const string SectionName = "GitHub";

    public string ApiBaseUrl { get; set; } = "https://api.github.com";
    public string PersonalAccessToken { get; set; } = "ghp_placeholder_token_replace";
    public string UserAgent { get; set; } = "github-favorites-app";

    // OAuth settings
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = "http://localhost:5092/auth/github/callback";
    public string FrontendCallbackUrl { get; set; } = "http://localhost:4200/auth/github/callback";
    public string OAuthBaseUrl { get; set; } = "https://github.com";
    public string OAuthApiBaseUrl { get; set; } = "https://api.github.com";
}

