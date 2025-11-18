using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using FavoritesAPI.Models.Auth;
using FavoritesAPI.Options;
using FavoritesAPI.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FavoritesAPI.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController(
    IAuthService authService,
    IJwtTokenService jwtTokenService,
    IHttpClientFactory httpClientFactory,
    IOptions<GitHubOptions> githubOptions,
    ILogger<AuthController> logger)
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

    [HttpGet("github")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public IActionResult InitiateGitHubOAuth()
    {
        var options = githubOptions.Value;
        if (string.IsNullOrWhiteSpace(options.ClientId))
        {
            logger.LogError("GitHub OAuth ClientId is not configured");
            return Problem(title: "GitHub OAuth is not configured", statusCode: StatusCodes.Status500InternalServerError);
        }

        var state = Guid.NewGuid().ToString();
        HttpContext.Session.SetString("github_oauth_state", state);

        var queryParams = new Dictionary<string, string?>
        {
            { "client_id", options.ClientId },
            { "redirect_uri", options.RedirectUri },
            { "scope", "user:email" },
            { "state", state }
        };
        var authorizeUrl = QueryHelpers.AddQueryString($"{options.OAuthBaseUrl}/login/oauth/authorize", queryParams);

        return Redirect(authorizeUrl);
    }

    [HttpGet("github/callback")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GitHubOAuthCallback(
        [FromQuery] string? code,
        [FromQuery] string? state,
        CancellationToken cancellationToken)
    {
        var options = githubOptions.Value;

        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(state))
        {
            logger.LogWarning("GitHub OAuth callback missing code or state");
            return BadRequest(new { error = "Missing authorization code or state" });
        }

        var storedState = HttpContext.Session.GetString("github_oauth_state");
        if (storedState != state)
        {
            logger.LogWarning("GitHub OAuth state mismatch");
            return BadRequest(new { error = "Invalid state parameter" });
        }

        HttpContext.Session.Remove("github_oauth_state");

        try
        {
            // Exchange code for access token
            var httpClient = httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            httpClient.DefaultRequestHeaders.Add("User-Agent", options.UserAgent);

            var tokenRequest = new Dictionary<string, string?>
            {
                { "client_id", options.ClientId },
                { "client_secret", options.ClientSecret },
                { "code", code },
                { "redirect_uri", options.RedirectUri }
            };

            using var tokenContent = new FormUrlEncodedContent(tokenRequest!);
            var tokenResponse = await httpClient.PostAsync(
                $"{options.OAuthBaseUrl}/login/oauth/access_token",
                tokenContent,
                cancellationToken);

            if (!tokenResponse.IsSuccessStatusCode)
            {
                logger.LogError("Failed to exchange GitHub OAuth code for token. Status: {Status}", tokenResponse.StatusCode);
                return Problem(title: "Failed to authenticate with GitHub", statusCode: StatusCodes.Status500InternalServerError);
            }

            var tokenPayload = await tokenResponse.Content.ReadAsStringAsync(cancellationToken);
            GitHubTokenResponse? tokenData = null;

            if (!string.IsNullOrWhiteSpace(tokenPayload))
            {
                var contentType = tokenResponse.Content.Headers.ContentType?.MediaType ?? string.Empty;
                if (contentType.Contains("json", StringComparison.OrdinalIgnoreCase) ||
                    tokenPayload.TrimStart().StartsWith("{", StringComparison.Ordinal))
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(tokenPayload);
                        var root = doc.RootElement;
                        tokenData = new GitHubTokenResponse
                        {
                            AccessToken = root.TryGetProperty("access_token", out var accessTokenElement)
                                ? accessTokenElement.GetString() ?? string.Empty
                                : string.Empty,
                            TokenType = root.TryGetProperty("token_type", out var tokenTypeElement)
                                ? tokenTypeElement.GetString()
                                : null,
                            Scope = root.TryGetProperty("scope", out var scopeElement)
                                ? scopeElement.GetString()
                                : null
                        };
                    }
                    catch (JsonException ex)
                    {
                        logger.LogWarning(ex, "Failed to parse JSON token response. Falling back to query parsing.");
                    }
                }
                else
                {
                    var parsed = QueryHelpers.ParseQuery(tokenPayload);
                    parsed.TryGetValue("access_token", out var accessTokenValues);
                    parsed.TryGetValue("token_type", out var tokenTypeValues);
                    parsed.TryGetValue("scope", out var scopeValues);

                    tokenData = new GitHubTokenResponse
                    {
                        AccessToken = accessTokenValues.FirstOrDefault() ?? string.Empty,
                        TokenType = tokenTypeValues.FirstOrDefault(),
                        Scope = scopeValues.FirstOrDefault()
                    };
                }
            }
            if (tokenData is null || string.IsNullOrWhiteSpace(tokenData.AccessToken))
            {
                logger.LogError("Invalid token response from GitHub. Payload: {Payload}", tokenPayload);
                return Problem(title: "Failed to authenticate with GitHub", statusCode: StatusCodes.Status500InternalServerError);
            }

            // Get user info from GitHub
            var userClient = httpClientFactory.CreateClient();
            userClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {tokenData.AccessToken}");
            userClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
            userClient.DefaultRequestHeaders.Add("User-Agent", options.UserAgent);

            var userResponse = await userClient.GetAsync($"{options.OAuthApiBaseUrl}/user", cancellationToken);
            if (!userResponse.IsSuccessStatusCode)
            {
                logger.LogError("Failed to get GitHub user info. Status: {Status}", userResponse.StatusCode);
                return Problem(title: "Failed to get user information from GitHub", statusCode: StatusCodes.Status500InternalServerError);
            }

            var githubUser = await userResponse.Content.ReadFromJsonAsync<GitHubUserResponse>(cancellationToken: cancellationToken);
            if (githubUser is null)
            {
                logger.LogError("Invalid user response from GitHub");
                return Problem(title: "Failed to get user information from GitHub", statusCode: StatusCodes.Status500InternalServerError);
            }

            // Get user email (primary email)
            string email = githubUser.Email ?? string.Empty;
            if (string.IsNullOrWhiteSpace(email))
            {
                // Try to get email from emails endpoint
                var emailsResponse = await userClient.GetAsync($"{options.OAuthApiBaseUrl}/user/emails", cancellationToken);
                if (emailsResponse.IsSuccessStatusCode)
                {
                    var emails = await emailsResponse.Content.ReadFromJsonAsync<List<GitHubEmailResponse>>(cancellationToken: cancellationToken);
                    var primaryEmail = emails?.FirstOrDefault(e => e.Primary && e.Verified);
                    email = primaryEmail?.Email ?? emails?.FirstOrDefault(e => e.Verified)?.Email ?? string.Empty;
                }
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                logger.LogError("Unable to retrieve email from GitHub");
                return Problem(title: "Unable to retrieve email from GitHub account", statusCode: StatusCodes.Status400BadRequest);
            }

            // Create or update user
            var user = await authService.CreateOrUpdateGitHubUserAsync(
                githubUser.Id,
                email,
                githubUser.Login,
                githubUser.AvatarUrl ?? string.Empty,
                tokenData.AccessToken,
                cancellationToken);

            var loginResponse = jwtTokenService.GenerateToken(user);

            // Redirect to frontend with token and user info
            var frontendQueryParams = new Dictionary<string, string?>
            {
                { "token", loginResponse.Token },
                { "email", loginResponse.Email },
                { "username", loginResponse.Username ?? string.Empty },
                { "avatarUrl", loginResponse.AvatarUrl ?? string.Empty }
            };
            var frontendUrl = QueryHelpers.AddQueryString(options.FrontendCallbackUrl, frontendQueryParams);
            return Redirect(frontendUrl);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during GitHub OAuth callback");
            return Problem(title: "An error occurred during authentication", statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private sealed class GitHubTokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string? TokenType { get; set; }
        public string? Scope { get; set; }
    }

    private sealed class GitHubUserResponse
    {
        public long Id { get; set; }
        public string Login { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? AvatarUrl { get; set; }
    }

    private sealed class GitHubEmailResponse
    {
        public string Email { get; set; } = string.Empty;
        public bool Primary { get; set; }
        public bool Verified { get; set; }
    }
}

