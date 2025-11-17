using Microsoft.EntityFrameworkCore;
using RepositoryAnalysisWorker.Data;
using RepositoryAnalysisWorker.Options;
using RepositoryAnalysisWorker.Services;
using RepositoryAnalysisWorker.Services.Contracts;
using RepositoryAnalysisWorker.Workers;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        services.Configure<GitHubOptions>(configuration.GetSection(GitHubOptions.SectionName));
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("Postgres"));
        });

        services.AddHttpClient<IGitHubMetadataService, GitHubMetadataService>();
        services.AddScoped<IRepositoryAnalysisService, RepositoryAnalysisService>();
        services.AddHostedService<FavoriteAnalysisWorker>();
    })
    .Build();

await host.RunAsync();

