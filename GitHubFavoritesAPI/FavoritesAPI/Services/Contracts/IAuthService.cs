using FavoritesAPI.Entities;

namespace FavoritesAPI.Services.Contracts;

public interface IAuthService
{
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken);
    Task<User?> FindByGitHubIdAsync(long githubId, CancellationToken cancellationToken);
    Task<bool> ValidatePasswordAsync(User user, string password);
    Task<User> CreateOrUpdateGitHubUserAsync(long githubId, string email, string username, string avatarUrl, string accessToken, CancellationToken cancellationToken);
}

