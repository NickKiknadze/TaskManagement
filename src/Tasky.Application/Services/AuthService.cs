using Microsoft.EntityFrameworkCore;
using Tasky.Application.DTOs;
using Tasky.Application.Interfaces;
using Tasky.Domain.Constants;
using Tasky.Domain.Entities;

namespace Tasky.Application.Services;

public class AuthService : IAuthService
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenGenerator _jwtGenerator;

    public AuthService(IApplicationDbContext context, IJwtTokenGenerator jwtGenerator)
    {
        _context = context;
        _jwtGenerator = jwtGenerator;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role!)
            .ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission!)
            .FirstOrDefaultAsync(u => u.Username == request.Username, cancellationToken);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }
        
        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("User is inactive");
        }

        var roles = user.UserRoles.Select(ur => ur.Role!.Name).Distinct().ToList();
        
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role!.RolePermissions)
            .Select(rp => rp.Permission!.Key)
            .Distinct()
            .ToList();

        var token = _jwtGenerator.GenerateToken(user, roles, permissions);
        var refreshToken = Guid.NewGuid().ToString(); 

        return new LoginResponse(token, refreshToken, new UserDto(user.Id.ToString(), user.Username, roles, permissions));
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (await _context.Users.AnyAsync(u => u.Username == request.Username, cancellationToken))
        {
            throw new InvalidOperationException("Username already exists");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        
        var user = new User
        {
            Username = request.Username,
            PasswordHash = passwordHash,
            IsActive = true
        };

        var memberRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == Roles.Member, cancellationToken);
        if (memberRole is not null)
        {
             user.UserRoles.Add(new UserRole { Role = memberRole });
        }

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        return new RegisterResponse(user.Id.ToString());
    }
}
