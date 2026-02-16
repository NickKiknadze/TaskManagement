using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Tasky.Application.Interfaces;
using Tasky.Domain.Entities;

namespace Tasky.Infrastructure.Authentication;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtOptions _jwtOptions;

    public JwtTokenGenerator(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
    }

    public string GenerateToken(User user, IEnumerable<string> roles, IEnumerable<string> permissions)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Name, user.Username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        
        // Add permissions as custom claims or reuse Role claim? 
        // Best practice: "permissions" claim or check in Policy via database.
        // Req says: "Login returns JWT + refresh token". "Role-based permissions (RBAC) with role-permission mapping".
        // "Policies: RequirePermission("tasks.create")". 
        // If we want stateless permission checks, we embed permissions in token.
        // Or we embed Roles, and the App resolves Permissions from Roles (caching).
        // Since we have Orleans/Redis Caching, checking permissions at runtime is fast.
        // But standard JWT usually embeds permissions or roles.
        // Let's embed permissions for simplicity in "permissions" claim.
        
        claims.AddRange(permissions.Select(permission => new Claim("permissions", permission)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
