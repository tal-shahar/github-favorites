using FavoritesAPI.Options;
using FavoritesAPI.Services.Contracts;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace FavoritesAPI.Services;

public sealed class RedisRateLimiter : IRateLimiter
{
    private readonly IDatabase _database;
    private readonly RateLimitOptions _options;

    public RedisRateLimiter(IConnectionMultiplexer multiplexer, IOptions<RateLimitOptions> options)
    {
        _database = multiplexer.GetDatabase();
        _options = options.Value;
    }

    public async Task<bool> IsAllowedAsync(Guid userId, CancellationToken cancellationToken)
    {
        var key = $"rate:search:{userId}";
        var count = await _database.StringIncrementAsync(key);

        if (count == 1)
        {
            await _database.KeyExpireAsync(key, TimeSpan.FromMinutes(1));
        }

        return count <= _options.SearchRequestsPerMinute;
    }
}

