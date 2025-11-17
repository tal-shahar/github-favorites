using FavoritesAPI.Models.Search;

namespace FavoritesAPI.Services.Contracts;

public interface IGitHubSearchService
{
    Task<IReadOnlyCollection<RepositorySearchResult>> SearchAsync(string query, int page, int perPage, CancellationToken cancellationToken);
}

