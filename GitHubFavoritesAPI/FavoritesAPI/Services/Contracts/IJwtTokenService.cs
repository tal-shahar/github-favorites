using FavoritesAPI.Entities;
using FavoritesAPI.Models.Auth;

namespace FavoritesAPI.Services.Contracts;

public interface IJwtTokenService
{
    LoginResponse GenerateToken(User user);
}

