using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FavoritesAPI.Entities;
using FavoritesAPI.Models.Auth;
using FavoritesAPI.Options;
using FavoritesAPI.Services.Contracts;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FavoritesAPI.Services;

public sealed class JwtTokenService(IOptions<JwtOptions> options) : IJwtTokenService
{
    private readonly JwtOptions _options = options.Value;

    public LoginResponse GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes);

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateJwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            subject: new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email)
            }),
            expires: expires,
            signingCredentials: creds);

        return new LoginResponse
        {
            Token = handler.WriteToken(token),
            ExpiresAtUtc = expires
        };
    }
}

