using FavoritesAPI.Extensions;
using FavoritesAPI.Models.Search;
using FavoritesAPI.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FavoritesAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/search")]
public sealed class SearchController(
    IGitHubSearchService gitHubSearchService,
    ICacheService cacheService,
    IRateLimiter rateLimiter,
    ILogger<SearchController> logger) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<RepositorySearchResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] SearchQuery query, CancellationToken cancellationToken)
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

        if (!await rateLimiter.IsAllowedAsync(userId, cancellationToken))
        {
            return Problem(
                title: "Rate limit exceeded",
                detail: "You may perform up to 10 searches per minute.",
                statusCode: StatusCodes.Status429TooManyRequests);
        }

        var cacheKey = $"search:{query.Q}:{query.Page}:{query.PerPage}";
        var cached = await cacheService.GetAsync<IReadOnlyCollection<RepositorySearchResult>>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return Ok(cached);
        }

        var results = await gitHubSearchService.SearchAsync(query.Q, query.Page, query.PerPage, cancellationToken);

        await cacheService.SetAsync(cacheKey, results, cacheService.DefaultSearchTtl, cancellationToken);
        logger.LogInformation("Search cache miss for query {Query}", query.Q);

        return Ok(results);
    }
}

