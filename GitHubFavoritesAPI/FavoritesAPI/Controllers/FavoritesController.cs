using FavoritesAPI.Extensions;
using FavoritesAPI.Mapping;
using FavoritesAPI.Models.Favorites;
using FavoritesAPI.Messaging.Events;
using FavoritesAPI.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FavoritesAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/favorites")]
public sealed class FavoritesController(
    IFavoriteService favoriteService,
    IRabbitMqPublisher publisher,
    ILogger<FavoritesController> logger) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> Create([FromBody] FavoriteRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var userId = User.GetUserId();
        if (userId == Guid.Empty)
        {
            return Problem(title: "Invalid token", statusCode: StatusCodes.Status401Unauthorized);
        }

        var (favorite, created) = await favoriteService.AddFavoriteAsync(userId, request, cancellationToken);

        if (created)
        {
            publisher.PublishFavoriteEvent(new FavoriteFavoritedEvent
            {
                FavoriteId = favorite.Id,
                RepoId = favorite.RepoId,
                UserId = userId,
                Name = favorite.Name,
                Owner = favorite.Owner,
                Stars = favorite.Stars,
                RepoUpdatedAtUtc = favorite.RepoUpdatedAtUtc
            });

            logger.LogInformation("Favorite {FavoriteId} enqueued for analysis", favorite.Id);
        }

        return Accepted(new { favorite.Id });
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<FavoriteResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == Guid.Empty)
        {
            return Problem(title: "Invalid token", statusCode: StatusCodes.Status401Unauthorized);
        }

        var favorites = await favoriteService.GetFavoritesAsync(userId, cancellationToken);
        return Ok(favorites.Select(f => f.ToResponse()));
    }

    [HttpDelete("{repoId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string repoId, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == Guid.Empty)
        {
            return Problem(title: "Invalid token", statusCode: StatusCodes.Status401Unauthorized);
        }

        var removed = await favoriteService.RemoveFavoriteAsync(userId, repoId, cancellationToken);
        if (!removed)
        {
            return NotFound();
        }

        return NoContent();
    }
}

