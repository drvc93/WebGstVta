using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GestVta.Api.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GestVta.Api.Infrastructure;

public sealed record AccessTokenResult(string Token, DateTime ExpiresAtUtc);

public sealed class JwtTokenService(IOptions<JwtOptions> options)
{
    private readonly JwtOptions _opts = options.Value;

    public AccessTokenResult CreateAccessToken(Usuario user, IReadOnlyList<string> roles, string colorPrimario, string? companiaNombre, int? companiaIdEfectiva = null)
    {
        var keyBytes = Encoding.UTF8.GetBytes(_opts.Key);
        if (keyBytes.Length < 32)
            throw new InvalidOperationException("Jwt:Key debe codificar al menos 32 bytes UTF-8 (HS256).");

        var signingKey = new SymmetricSecurityKey(keyBytes);
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(ClaimTypes.GivenName, user.NombreMostrar ?? string.Empty),
            new("compania_id", (companiaIdEfectiva ?? user.CompaniaId)?.ToString() ?? string.Empty),
            new("compania_nombre", companiaNombre ?? string.Empty),
            new("color_primario", colorPrimario),
        };

        foreach (var r in roles)
            claims.Add(new Claim(ClaimTypes.Role, r));

        var expires = DateTime.UtcNow.AddMinutes(_opts.AccessTokenMinutes);
        var token = new JwtSecurityToken(
            issuer: _opts.Issuer,
            audience: _opts.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return new AccessTokenResult(tokenString, expires);
    }
}
