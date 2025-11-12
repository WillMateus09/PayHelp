using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PayHelp.Domain.Entities;

namespace PayHelp.Api.Services;

public class JwtOptions
{
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public int ExpiresMinutes { get; set; } = 120;
}

public interface IJwtTokenService
{
    (string token, DateTime expiresAtUtc) CreateToken(User user);
}

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _opts;
    public JwtTokenService(IOptions<JwtOptions> opts)
    {
        _opts = opts.Value;
    }

    public (string token, DateTime expiresAtUtc) CreateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opts.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(_opts.ExpiresMinutes);
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim("name", user.Nome ?? string.Empty),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };
        var token = new JwtSecurityToken(
            issuer: _opts.Issuer,
            audience: _opts.Audience,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: creds
        );
        var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);
        return (tokenStr, expires);
    }
}
