namespace FavoritesAPI.Options;

public sealed class RedisOptions
{
    public const string SectionName = "Redis";

    public string ConnectionString { get; set; } = "localhost:6379";
    public int SearchCacheTtlSeconds { get; set; } = 90;
}

