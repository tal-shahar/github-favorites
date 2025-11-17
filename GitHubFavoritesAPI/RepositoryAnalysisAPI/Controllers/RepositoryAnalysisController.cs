using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepositoryAnalysisAPI.Data;
using RepositoryAnalysisAPI.Mapping;
using RepositoryAnalysisAPI.Models;

namespace RepositoryAnalysisAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/analysis")]
public sealed class RepositoryAnalysisController(AppDbContext dbContext, ILogger<RepositoryAnalysisController> logger) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<AnalysisResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] Guid? userId, CancellationToken cancellationToken)
    {
        var query = dbContext.RepositoryAnalyses
            .Include(a => a.Favorite)
            .AsQueryable();

        if (userId.HasValue)
        {
            query = query.Where(a => a.Favorite!.UserId == userId);
        }

        var items = await query
            .OrderByDescending(a => a.CreatedAtUtc)
            .Take(100)
            .ToListAsync(cancellationToken);

        return Ok(items.Select(x => x.ToResponse()));
    }

    [HttpGet("{favoriteId:guid}")]
    [ProducesResponseType(typeof(AnalysisResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid favoriteId, CancellationToken cancellationToken)
    {
        var analysis = await dbContext.RepositoryAnalyses
            .Include(a => a.Favorite)
            .SingleOrDefaultAsync(a => a.FavoriteId == favoriteId, cancellationToken);

        if (analysis is null)
        {
            return NotFound();
        }

        return Ok(analysis.ToResponse());
    }

    [HttpGet("by-repo/{repoId}")]
    [ProducesResponseType(typeof(AnalysisResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByRepo(string repoId, CancellationToken cancellationToken)
    {
        var analysis = await dbContext.RepositoryAnalyses
            .Include(a => a.Favorite)
            .SingleOrDefaultAsync(a => a.Favorite!.RepoId == repoId, cancellationToken);

        if (analysis is null)
        {
            logger.LogInformation("Analysis not found for repo {RepoId}", repoId);
            return NotFound();
        }

        return Ok(analysis.ToResponse());
    }
}

