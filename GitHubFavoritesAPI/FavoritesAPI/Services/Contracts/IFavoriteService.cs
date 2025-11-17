using FavoritesAPI.Entities;
using FavoritesAPI.Models.Favorites;

namespace FavoritesAPI.Services.Contracts;

public interface IFavoriteService
{
    Task<(Favorite Favorite, bool Created)> AddFavoriteAsync(Guid userId, FavoriteRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Favorite>> GetFavoritesAsync(Guid userId, CancellationToken cancellationToken);
    Task<bool> RemoveFavoriteAsync(Guid userId, string repoId, CancellationToken cancellationToken);
}

