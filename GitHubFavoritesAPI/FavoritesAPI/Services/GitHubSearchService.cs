using System.Text.Json;
using FavoritesAPI.Models.Search;
using FavoritesAPI.Options;
using FavoritesAPI.Services.Contracts;
using Microsoft.Extensions.Options;
using System.Text.Json.Serialization;

namespace FavoritesAPI.Services;

public sealed class GitHubSearchService : IGitHubSearchService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GitHubSearchService> _logger;
    private readonly GitHubOptions _options;

    public GitHubSearchService(HttpClient httpClient, IOptions<GitHubOptions> options, ILogger<GitHubSearchService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value;

        _httpClient.BaseAddress ??= new Uri(_options.ApiBaseUrl);
        if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", _options.UserAgent);
        }

        if (!_httpClient.DefaultRequestHeaders.Contains("Accept"))
        {
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
        }

        if (!string.IsNullOrWhiteSpace(_options.PersonalAccessToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.PersonalAccessToken);
        }
    }

    public async Task<IReadOnlyCollection<RepositorySearchResult>> SearchAsync(string query, int page, int perPage, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync($"/search/repositories?q={Uri.EscapeDataString(query)}&page={page}&per_page={perPage}",
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var payload = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("GitHub search failed with status {Status} - {Body}", response.StatusCode, payload);
            throw new InvalidOperationException("GitHub search failed");
        }

        var json = await response.Content.ReadAsStreamAsync(cancellationToken);
        var result = await JsonSerializer.DeserializeAsync<GitHubSearchResponse>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                cancellationToken)
            ?? new GitHubSearchResponse();

        return result.Items.Select(item => new RepositorySearchResult
        {
            RepoId = item.Id.ToString(),
            Name = item.Name,
            Owner = item.Owner.Login,
            Description = item.Description ?? string.Empty,
            Stars = item.StargazersCount,
            UpdatedAtUtc = item.UpdatedAt
        }).ToList();
    }

    private sealed record GitHubSearchResponse([property: JsonPropertyName("items")] IReadOnlyList<GitHubRepository> Items)
    {
        public GitHubSearchResponse() : this(Array.Empty<GitHubRepository>())
        {
        }
    }

    private sealed record GitHubRepository(
        [property: JsonPropertyName("id")] long Id,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("owner")] GitHubOwner Owner,
        [property: JsonPropertyName("stargazers_count")] int StargazersCount,
        [property: JsonPropertyName("updated_at")] DateTime UpdatedAt,
        [property: JsonPropertyName("description")] string? Description);

    private sealed record GitHubOwner([property: JsonPropertyName("login")] string Login);
}

