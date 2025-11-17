using FavoritesAPI.Data;
using FavoritesAPI.Entities;
using FavoritesAPI.Models.Favorites;
using FavoritesAPI.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FavoritesAPI.Services;

public sealed class FavoriteService(AppDbContext dbContext, ILogger<FavoriteService> logger) : IFavoriteService
{
    public async Task<(Favorite Favorite, bool Created)> AddFavoriteAsync(Guid userId, FavoriteRequest request, CancellationToken cancellationToken)
    {
        var existing = await dbContext.Favorites
            .SingleOrDefaultAsync(f => f.UserId == userId && f.RepoId == request.RepoId, cancellationToken);

        if (existing is not null)
        {
            return (existing, false);
        }

        var favorite = new Favorite
        {
            UserId = userId,
            RepoId = request.RepoId,
            Name = request.Name,
            Owner = request.Owner,
            Description = request.Description,
            Stars = request.Stars,
            RepoUpdatedAtUtc = request.UpdatedAtUtc
        };

        dbContext.Favorites.Add(favorite);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Favorite {RepoId} added for user {UserId}", favorite.RepoId, userId);
        return (favorite, true);
    }

    public async Task<IReadOnlyCollection<Favorite>> GetFavoritesAsync(Guid userId, CancellationToken cancellationToken)
    {
        var favorites = await dbContext.Favorites
            .Where(f => f.UserId == userId)
            .Include(f => f.Analysis)
            .OrderByDescending(f => f.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return favorites;
    }

    public async Task<bool> RemoveFavoriteAsync(Guid userId, string repoId, CancellationToken cancellationToken)
    {
        var favorite = await dbContext.Favorites
            .SingleOrDefaultAsync(f => f.UserId == userId && f.RepoId == repoId, cancellationToken);

        if (favorite is null)
        {
            return false;
        }

        dbContext.Favorites.Remove(favorite);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}

