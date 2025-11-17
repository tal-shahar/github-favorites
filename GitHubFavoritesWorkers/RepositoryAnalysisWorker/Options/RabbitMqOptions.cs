namespace RepositoryAnalysisWorker.Options;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string VirtualHost { get; set; } = "/";
    public string UserName { get; set; } = "github_favorites";
    public string Password { get; set; } = "github_favorites_password";
    public string Exchange { get; set; } = "repo.events";
    public string Queue { get; set; } = "repo.favorited.queue";
    public string RetryExchange { get; set; } = "repo.favorited.retry";
    public string RetryQueue { get; set; } = "repo.favorited.retry.queue";
    public string DeadLetterExchange { get; set; } = "repo.favorited.dlq";
    public string DeadLetterQueue { get; set; } = "repo.favorited.dlq.queue";
    public string FavoritedRoutingKey { get; set; } = "repo.favorited";
    public string AnalysisReadyRoutingKey { get; set; } = "analysis.ready";
}

