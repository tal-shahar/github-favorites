using FavoritesAPI.Entities;

namespace FavoritesAPI.Services.Contracts;

public interface IAuthService
{
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken);
    Task<bool> ValidatePasswordAsync(User user, string password);
}

