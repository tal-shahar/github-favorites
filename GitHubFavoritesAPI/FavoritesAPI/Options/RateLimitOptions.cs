namespace FavoritesAPI.Options;

public sealed class RateLimitOptions
{
    public const string SectionName = "RateLimit";

    public int SearchRequestsPerMinute { get; set; } = 10;
}

