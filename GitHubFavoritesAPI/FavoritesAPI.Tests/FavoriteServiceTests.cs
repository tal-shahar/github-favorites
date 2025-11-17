using FavoritesAPI.Data;
using FavoritesAPI.Models.Favorites;
using FavoritesAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace FavoritesAPI.Tests;

public class FavoriteServiceTests
{
    [Fact]
    public async Task AddFavorite_ReturnsExistingWhenRepoAlreadyTracked()
    {
        // Arrange
        var service = CreateService(out var dbContext);
        var userId = Guid.NewGuid();
        var request = new FavoriteRequest
        {
            RepoId = "123",
            Name = "demo",
            Owner = "octocat",
            Description = "test",
            Stars = 10,
            UpdatedAtUtc = DateTime.UtcNow
        };

        await service.AddFavoriteAsync(userId, request, CancellationToken.None);

        // Act
        var (_, created) = await service.AddFavoriteAsync(userId, request, CancellationToken.None);

        // Assert
        Assert.False(created);
        Assert.Single(dbContext.Favorites);
    }

    private static FavoriteService CreateService(out AppDbContext dbContext)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        dbContext = new AppDbContext(options);
        return new FavoriteService(dbContext, NullLogger<FavoriteService>.Instance);
    }
}

