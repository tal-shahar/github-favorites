using FavoritesAPI.Data;
using FavoritesAPI.Entities;
using FavoritesAPI.Services.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FavoritesAPI.Services;

public sealed class AuthService(AppDbContext dbContext, IPasswordHasher<User> passwordHasher)
    : IAuthService
{
    public Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken)
        => dbContext.Users.SingleOrDefaultAsync(u => u.Email == email, cancellationToken);

    public Task<User?> FindByGitHubIdAsync(long githubId, CancellationToken cancellationToken)
        => dbContext.Users.SingleOrDefaultAsync(u => u.GitHubId == githubId, cancellationToken);

    public Task<bool> ValidatePasswordAsync(User user, string password)
    {
        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        return Task.FromResult(result == PasswordVerificationResult.Success);
    }

    public async Task<User> CreateOrUpdateGitHubUserAsync(long githubId, string email, string username, string avatarUrl, string accessToken, CancellationToken cancellationToken)
    {
        var existingUser = await FindByGitHubIdAsync(githubId, cancellationToken);

        if (existingUser is not null)
        {
            // Update existing user
            existingUser.Email = email;
            existingUser.GitHubUsername = username;
            existingUser.AvatarUrl = avatarUrl;
            existingUser.AccessToken = accessToken;
            await dbContext.SaveChangesAsync(cancellationToken);
            return existingUser;
        }

        // Check if user with this email exists
        var userByEmail = await FindByEmailAsync(email, cancellationToken);
        if (userByEmail is not null)
        {
            // Link GitHub account to existing user
            userByEmail.GitHubId = githubId;
            userByEmail.GitHubUsername = username;
            userByEmail.AvatarUrl = avatarUrl;
            userByEmail.AccessToken = accessToken;
            await dbContext.SaveChangesAsync(cancellationToken);
            return userByEmail;
        }

        // Create new user
        var newUser = new User
        {
            Email = email,
            GitHubId = githubId,
            GitHubUsername = username,
            AvatarUrl = avatarUrl,
            AccessToken = accessToken,
            PasswordHash = string.Empty // No password for GitHub OAuth users
        };

        dbContext.Users.Add(newUser);
        await dbContext.SaveChangesAsync(cancellationToken);
        return newUser;
    }
}

