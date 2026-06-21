using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ShopEaseApp.Api.Infrastructure.Data;

namespace ShopEaseApp.Api.Infrastructure.Auth;

public class JwtService(IConfiguration configuration)
{
    private readonly IConfiguration _configuration = configuration;

  public string GenerateToken(AppUser user, IList<string> roles)
    {
        var jwtSection = _configuration.GetSection("Jwt");
        var key = jwtSection["Key"] ?? throw new InvalidOperationException("JWT Key not configured.");
        var issuer = jwtSection["Issuer"];
        var audience = jwtSection["Audience"];
        var expiryMinutes = int.Parse(jwtSection["ExpiryMinutes"] ?? "60");

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)) { KeyId = "ShopEase-default-key" };
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public DateTime GetExpiry()
    {
        var expiryMinutes = int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "60");
        return DateTime.UtcNow.AddMinutes(expiryMinutes);
    }
}
