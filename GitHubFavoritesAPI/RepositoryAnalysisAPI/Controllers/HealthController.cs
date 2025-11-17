using Microsoft.AspNetCore.Mvc;
using RepositoryAnalysisAPI.Data;

namespace RepositoryAnalysisAPI.Controllers;

[ApiController]
public sealed class HealthController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet("health")]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var dbHealthy = await dbContext.Database.CanConnectAsync(cancellationToken);
        var payload = new
        {
            status = dbHealthy ? "Healthy" : "Degraded",
            checks = new { database = dbHealthy }
        };

        return dbHealthy ? Ok(payload) : StatusCode(StatusCodes.Status503ServiceUnavailable, payload);
    }
}

