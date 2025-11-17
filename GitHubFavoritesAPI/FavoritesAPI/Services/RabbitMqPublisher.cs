using System.Text;
using System.Text.Json;
using FavoritesAPI.Messaging.Events;
using FavoritesAPI.Options;
using FavoritesAPI.Services.Contracts;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace FavoritesAPI.Services;

public sealed class RabbitMqPublisher : IRabbitMqPublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqPublisher> _logger;

    public RabbitMqPublisher(IOptions<RabbitMqOptions> options, ILogger<RabbitMqPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;

        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            VirtualHost = _options.VirtualHost,
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection("favorites-api");
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare(_options.Exchange, ExchangeType.Topic, durable: true, autoDelete: false);
    }

    public void PublishFavoriteEvent(FavoriteFavoritedEvent message)
    {
        var payload = JsonSerializer.SerializeToUtf8Bytes(message, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var props = _channel.CreateBasicProperties();
        props.ContentType = "application/json";
        props.DeliveryMode = 2;

        _channel.BasicPublish(_options.Exchange, _options.FavoritedRoutingKey, basicProperties: props, body: payload);
        _logger.LogInformation("Published repo.favorited event for favorite {FavoriteId}", message.FavoriteId);
    }

    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
    }
}

