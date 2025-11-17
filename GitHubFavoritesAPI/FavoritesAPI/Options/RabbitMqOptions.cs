namespace FavoritesAPI.Options;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string VirtualHost { get; set; } = "/";
    public string UserName { get; set; } = "github_favorites";
    public string Password { get; set; } = "github_favorites_password";
    public string Exchange { get; set; } = "repo.events";
    public string FavoritedRoutingKey { get; set; } = "repo.favorited";
}

