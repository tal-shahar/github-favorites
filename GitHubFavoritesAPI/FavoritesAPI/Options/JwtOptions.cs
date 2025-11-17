namespace FavoritesAPI.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "github-favorites-api";
    public string Audience { get; set; } = "github-favorites-clients";
    public string SigningKey { get; set; } = "local-dev-very-secret-signing-key-change-me";
    public int AccessTokenMinutes { get; set; } = 60;
}

