using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ShiftEngine.Infrastructure.Identity;

namespace ShiftEngine.Infrastructure.Auth;

public class JwtTokenIssuer(IConfiguration configuration, UserManager<AppUser> users)
{
    public async Task<string> CreateTokenAsync(AppUser user, IEnumerable<string> roles, CancellationToken ct = default)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? "dev-only-change-me-32chars-min!!"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var roleList = await users.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new("tenant_id", user.TenantId.ToString()),
            new("display_name", user.DisplayName)
        };
        claims.AddRange(roleList.Select(r => new Claim(ClaimTypes.Role, r)));
        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
