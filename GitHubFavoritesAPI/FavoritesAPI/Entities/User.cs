namespace FavoritesAPI.Entities;

public sealed class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    // GitHub OAuth fields
    public long? GitHubId { get; set; }
    public string? GitHubUsername { get; set; }
    public string? AvatarUrl { get; set; }
    public string? AccessToken { get; set; }

    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
}

