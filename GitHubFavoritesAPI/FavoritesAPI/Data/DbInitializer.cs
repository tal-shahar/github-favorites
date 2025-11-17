using FavoritesAPI.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FavoritesAPI.Data;

public static class DbInitializer
{
    private const string DefaultEmail = "demo@githubfavorites.local";
    private const string DefaultPassword = "ChangeMe123!";

    public static async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

        await context.Database.MigrateAsync(cancellationToken);

        if (!await context.Users.AnyAsync(cancellationToken))
        {
            var user = new User
            {
                Email = DefaultEmail
            };
            user.PasswordHash = passwordHasher.HashPassword(user, DefaultPassword);
            context.Users.Add(user);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}

