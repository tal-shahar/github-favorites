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

    public Task<bool> ValidatePasswordAsync(User user, string password)
    {
        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        return Task.FromResult(result == PasswordVerificationResult.Success);
    }
}

