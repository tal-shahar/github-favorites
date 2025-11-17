using System.Text.Json;
using FavoritesAPI.Options;
using FavoritesAPI.Services.Contracts;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace FavoritesAPI.Services;

public sealed class RedisCacheService : ICacheService
{
    private readonly IDatabase _database;
    private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);
    private readonly RedisOptions _options;

    public RedisCacheService(IConnectionMultiplexer multiplexer, IOptions<RedisOptions> options)
    {
        _database = multiplexer.GetDatabase();
        _options = options.Value;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken)
    {
        var value = await _database.StringGetAsync(key);
        if (value.IsNullOrEmpty)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(value!, _serializerOptions);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(value, _serializerOptions);
        return _database.StringSetAsync(key, json, ttl);
    }

    public TimeSpan DefaultSearchTtl => TimeSpan.FromSeconds(_options.SearchCacheTtlSeconds);
}

