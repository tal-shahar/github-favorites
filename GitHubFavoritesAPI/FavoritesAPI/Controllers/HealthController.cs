using FavoritesAPI.Data;
using FavoritesAPI.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using StackExchange.Redis;

namespace FavoritesAPI.Controllers;

[ApiController]
public sealed class HealthController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IConnectionMultiplexer _redis;
    private readonly RabbitMqOptions _rabbitOptions;

    public HealthController(AppDbContext dbContext, IConnectionMultiplexer redis, IOptions<RabbitMqOptions> rabbitOptions)
    {
        _dbContext = dbContext;
        _redis = redis;
        _rabbitOptions = rabbitOptions.Value;
    }

    [HttpGet("health")]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var dbHealthy = await CheckDatabaseAsync(cancellationToken);
        var redisHealthy = await CheckRedisAsync();
        var rabbitHealthy = CheckRabbitMq();

        var healthy = dbHealthy && redisHealthy && rabbitHealthy;

        var payload = new
        {
            status = healthy ? "Healthy" : "Degraded",
            checks = new
            {
                database = dbHealthy,
                redis = redisHealthy,
                rabbitmq = rabbitHealthy
            },
            timestamp = DateTime.UtcNow
        };

        return healthy ? Ok(payload) : StatusCode(StatusCodes.Status503ServiceUnavailable, payload);
    }

    private async Task<bool> CheckDatabaseAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _dbContext.Database.CanConnectAsync(cancellationToken);
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> CheckRedisAsync()
    {
        try
        {
            var pong = await _redis.GetDatabase().PingAsync();
            return pong >= TimeSpan.Zero;
        }
        catch
        {
            return false;
        }
    }

    private bool CheckRabbitMq()
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _rabbitOptions.HostName,
                Port = _rabbitOptions.Port,
                UserName = _rabbitOptions.UserName,
                Password = _rabbitOptions.Password,
                VirtualHost = _rabbitOptions.VirtualHost
            };

            using var connection = factory.CreateConnection("health-check");
            return connection.IsOpen;
        }
        catch
        {
            return false;
        }
    }
}

