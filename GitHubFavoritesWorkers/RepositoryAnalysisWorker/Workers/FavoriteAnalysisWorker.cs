using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RepositoryAnalysisWorker.Messaging.Events;
using RepositoryAnalysisWorker.Options;
using RepositoryAnalysisWorker.Services.Contracts;

namespace RepositoryAnalysisWorker.Workers;

public sealed class FavoriteAnalysisWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<FavoriteAnalysisWorker> _logger;
    private readonly RabbitMqOptions _options;
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public FavoriteAnalysisWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqOptions> options,
        ILogger<FavoriteAnalysisWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;

        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            VirtualHost = _options.VirtualHost,
            UserName = _options.UserName,
            Password = _options.Password,
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection("repository-analysis-worker");
        _channel = _connection.CreateModel();
        ConfigureTopology();
    }

    private void ConfigureTopology()
    {
        _channel.ExchangeDeclare(_options.Exchange, ExchangeType.Topic, durable: true);
        _channel.ExchangeDeclare(_options.RetryExchange, ExchangeType.Topic, durable: true);
        _channel.ExchangeDeclare(_options.DeadLetterExchange, ExchangeType.Fanout, durable: true);

        _channel.QueueDeclare(_options.Queue, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(_options.Queue, _options.Exchange, _options.FavoritedRoutingKey);

        var retryArgs = new Dictionary<string, object>
        {
            { "x-message-ttl", 10000 },
            { "x-dead-letter-exchange", _options.Exchange },
            { "x-dead-letter-routing-key", _options.FavoritedRoutingKey }
        };

        _channel.QueueDeclare(_options.RetryQueue, durable: true, exclusive: false, autoDelete: false, arguments: retryArgs);
        _channel.QueueBind(_options.RetryQueue, _options.RetryExchange, _options.FavoritedRoutingKey);

        _channel.QueueDeclare(_options.DeadLetterQueue, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(_options.DeadLetterQueue, _options.DeadLetterExchange, string.Empty);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += HandleMessageAsync;

        _channel.BasicQos(prefetchSize: 0, prefetchCount: 2, global: false);
        _channel.BasicConsume(_options.Queue, autoAck: false, consumer);

        _logger.LogInformation("Repository analysis worker started");
        return Task.CompletedTask;
    }

    private async Task HandleMessageAsync(object sender, BasicDeliverEventArgs eventArgs)
    {
        var body = eventArgs.Body.ToArray();
        FavoriteFavoritedEvent? message = null;

        try
        {
            message = JsonSerializer.Deserialize<FavoriteFavoritedEvent>(body, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            if (message is null)
            {
                throw new InvalidOperationException("Message payload invalid.");
            }

            using var scope = _scopeFactory.CreateScope();
            var metadataService = scope.ServiceProvider.GetRequiredService<IGitHubMetadataService>();
            var analysisService = scope.ServiceProvider.GetRequiredService<IRepositoryAnalysisService>();

            var metadata = await metadataService.FetchAsync(message.Owner, message.Name, CancellationToken.None);
            var analysis = await analysisService.UpsertAnalysisAsync(message.FavoriteId, metadata, CancellationToken.None);

            PublishAnalysisReady(message, analysis.HealthScore);

            _channel.BasicAck(eventArgs.DeliveryTag, multiple: false);
            _logger.LogInformation("Analysis ready for favorite {FavoriteId}", message.FavoriteId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process message");
            HandleFailure(eventArgs, message, ex);
        }
    }

    private void PublishAnalysisReady(FavoriteFavoritedEvent message, double healthScore)
    {
        var readyEvent = new AnalysisReadyEvent
        {
            FavoriteId = message.FavoriteId,
            RepoId = message.RepoId,
            UserId = message.UserId,
            HealthScore = healthScore,
            CreatedAtUtc = DateTime.UtcNow
        };

        var payload = JsonSerializer.SerializeToUtf8Bytes(readyEvent, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var props = _channel.CreateBasicProperties();
        props.ContentType = "application/json";
        props.DeliveryMode = 2;

        _channel.BasicPublish(_options.Exchange, _options.AnalysisReadyRoutingKey, props, payload);
    }

    private void HandleFailure(BasicDeliverEventArgs eventArgs, FavoriteFavoritedEvent? message, Exception exception)
    {
        eventArgs.BasicProperties ??= _channel.CreateBasicProperties();
        var retryCount = GetRetryCount(eventArgs.BasicProperties.Headers);
        var headers = new Dictionary<string, object>(eventArgs.BasicProperties.Headers ?? new Dictionary<string, object>())
        {
            ["x-retry-count"] = retryCount + 1
        };

        var props = _channel.CreateBasicProperties();
        props.Headers = headers;
        props.ContentType = "application/json";
        props.DeliveryMode = 2;

        if (retryCount < 3)
        {
            _channel.BasicPublish(_options.RetryExchange, _options.FavoritedRoutingKey, props, eventArgs.Body);
            _logger.LogWarning("Message re-queued for retry {Retry}", retryCount + 1);
        }
        else
        {
            _channel.BasicPublish(_options.DeadLetterExchange, routingKey: string.Empty, props, eventArgs.Body);
            _logger.LogError(exception, "Message moved to DLQ after {Retry} attempts", retryCount);
        }

        _channel.BasicAck(eventArgs.DeliveryTag, multiple: false);
    }

    private static int GetRetryCount(IDictionary<string, object>? headers)
    {
        if (headers is null)
        {
            return 0;
        }

        if (headers.TryGetValue("x-retry-count", out var value))
        {
            if (value is byte[] bytes)
            {
                var asString = Encoding.UTF8.GetString(bytes);
                return int.TryParse(asString, out var parsed) ? parsed : 0;
            }

            if (value is int retryInt)
            {
                return retryInt;
            }
        }

        return 0;
    }

    public override void Dispose()
    {
        _channel.Close();
        _connection.Close();
        base.Dispose();
    }
}

