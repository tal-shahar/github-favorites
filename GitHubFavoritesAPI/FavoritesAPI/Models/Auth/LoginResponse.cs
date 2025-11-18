namespace FavoritesAPI.Models.Auth;

public sealed class LoginResponse
{
    public string Token { get; init; } = string.Empty;
    public DateTime ExpiresAtUtc { get; init; }
    public string Email { get; init; } = string.Empty;
    public string? Username { get; init; }
    public string? AvatarUrl { get; init; }
}

