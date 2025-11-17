using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RepositoryAnalysisWorker.Models;
using RepositoryAnalysisWorker.Options;
using RepositoryAnalysisWorker.Services.Contracts;

namespace RepositoryAnalysisWorker.Services;

public sealed class GitHubMetadataService : IGitHubMetadataService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GitHubMetadataService> _logger;

    public GitHubMetadataService(HttpClient httpClient, IOptions<GitHubOptions> options, ILogger<GitHubMetadataService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        var settings = options.Value;

        _httpClient.BaseAddress ??= new Uri(settings.ApiBaseUrl);
        if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(settings.UserAgent);
        }
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

        if (!string.IsNullOrWhiteSpace(settings.PersonalAccessToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", settings.PersonalAccessToken);
        }
    }

    public async Task<RepositoryMetadata> FetchAsync(string owner, string name, CancellationToken cancellationToken)
    {
        var repo = await GetAsync<RepositoryResponse>($"/repos/{owner}/{name}", cancellationToken);
        var topicsResponse = await GetAsync<TopicsResponse>($"/repos/{owner}/{name}/topics", cancellationToken);
        var languages = await GetAsync<Dictionary<string, long>>($"/repos/{owner}/{name}/languages", cancellationToken);
        var readmeLength = await FetchReadmeLength(owner, name, cancellationToken);

        var metadata = new RepositoryMetadata
        {
            License = repo.License?.SpdxId ?? string.Empty,
            Topics = topicsResponse.Names,
            Languages = languages,
            PrimaryLanguage = repo.Language ?? string.Empty,
            ReadmeLength = readmeLength,
            OpenIssues = repo.OpenIssuesCount,
            Forks = repo.ForksCount,
            StarsSnapshot = repo.StargazersCount,
            ActivityDays = (int)Math.Max(0, (DateTime.UtcNow - repo.PushedAt.ToUniversalTime()).TotalDays),
            DefaultBranch = repo.DefaultBranch,
            RetrievedAtUtc = DateTime.UtcNow
        };

        return metadata;
    }

    private async Task<int> FetchReadmeLength(string owner, string name, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync($"/repos/{owner}/{name}/readme", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to fetch README for {Owner}/{Repo}: {Status}", owner, name, response.StatusCode);
            return 0;
        }

        var json = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<ReadmeResponse>(json, cancellationToken: cancellationToken);
        if (payload is null)
        {
            return 0;
        }

        if (payload.Content is null)
        {
            return payload.Size;
        }

        var buffer = Convert.FromBase64String(payload.Content);
        return Encoding.UTF8.GetString(buffer).Length;
    }

    private async Task<T> GetAsync<T>(string path, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync(path, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("GitHub call to {Path} failed with {Status}: {Body}", path, response.StatusCode, body);
            throw new InvalidOperationException($"GitHub request failed: {path}");
        }

        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<T>(stream, cancellationToken: cancellationToken);
        if (payload is null)
        {
            throw new InvalidOperationException($"Unable to parse GitHub response for {path}");
        }

        return payload;
    }

    private sealed record RepositoryResponse(
        int StargazersCount,
        int OpenIssuesCount,
        int ForksCount,
        DateTime PushedAt,
        string DefaultBranch,
        string? Language,
        LicenseInfo? License);

    private sealed record LicenseInfo(string? SpdxId);

    private sealed record TopicsResponse(List<string> Names);

    private sealed record ReadmeResponse(int Size, string? Content);
}

