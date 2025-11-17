using FavoritesAPI.Models.Auth;
using FavoritesAPI.Services.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace FavoritesAPI.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController(IAuthService authService, IJwtTokenService jwtTokenService, ILogger<AuthController> logger)
    : ControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var user = await authService.FindByEmailAsync(request.Email, cancellationToken);
        if (user is null)
        {
            logger.LogWarning("Login failed for {Email}", request.Email);
            return Problem(title: "Invalid credentials", statusCode: StatusCodes.Status401Unauthorized);
        }

        var valid = await authService.ValidatePasswordAsync(user, request.Password);
        if (!valid)
        {
            logger.LogWarning("Invalid password for {Email}", request.Email);
            return Problem(title: "Invalid credentials", statusCode: StatusCodes.Status401Unauthorized);
        }

        var response = jwtTokenService.GenerateToken(user);
        return Ok(response);
    }
}

