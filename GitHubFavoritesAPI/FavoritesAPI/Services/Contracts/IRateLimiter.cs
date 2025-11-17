namespace FavoritesAPI.Services.Contracts;

public interface IRateLimiter
{
    Task<bool> IsAllowedAsync(Guid userId, CancellationToken cancellationToken);
}

