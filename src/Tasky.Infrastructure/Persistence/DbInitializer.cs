using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tasky.Domain.Entities;
using Tasky.Domain.Constants;

namespace Tasky.Infrastructure.Persistence;

public class DbInitializer
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DbInitializer> _logger;

    public DbInitializer(ApplicationDbContext context, ILogger<DbInitializer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task InitializeAsync(string defaultAdminPassword)
    {
        try
        {
            if (_context.Database.IsSqlServer())
            {
                await _context.Database.MigrateAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }

        await SeedAsync(defaultAdminPassword);
    }

    private async Task SeedAsync(string defaultAdminPassword)
    {
        try
        {
            if (!await _context.Roles.AnyAsync())
            {
                await SeedRolesAndPermissionsAsync();
            }

            if (!await _context.Users.AnyAsync())
            {
                await SeedUsersAsync(defaultAdminPassword);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private async Task SeedRolesAndPermissionsAsync()
    {
        var allPermissions = Permissions.All().Select(p => new Permission
        {
            Key = p,
            Description = $"Permission to {p}"
        }).ToList();
        
        await _context.Permissions.AddRangeAsync(allPermissions);
        await _context.SaveChangesAsync();

        var permissionMap = await _context.Permissions.ToDictionaryAsync(p => p.Key, p => p);

        var adminRole = new Role { Name = Roles.Admin };
        var pmRole = new Role { Name = Roles.ProjectManager };
        var memberRole = new Role { Name = Roles.Member };
        var viewerRole = new Role { Name = Roles.Viewer };

        _context.Roles.AddRange(adminRole, pmRole, memberRole, viewerRole);
        await _context.SaveChangesAsync();

        foreach (var p in allPermissions)
        {
            adminRole.RolePermissions.Add(new RolePermission { RoleId = adminRole.Id, PermissionId = p.Id });
        }

        foreach (var p in allPermissions.Where(x => !x.Key.StartsWith("users.") && !x.Key.StartsWith("roles.")))
        {
            pmRole.RolePermissions.Add(new RolePermission { RoleId = pmRole.Id, PermissionId = p.Id });
        }
        var pmPermissions = allPermissions.Where(p => 
            !p.Key.Equals(Permissions.Users.Manage) && 
            !p.Key.Equals(Permissions.Roles.Manage)).ToList();
            
        pmRole.RolePermissions.Clear();
        foreach(var p in pmPermissions)
        {
             pmRole.RolePermissions.Add(new RolePermission { RoleId = pmRole.Id, PermissionId = p.Id });
        }

        var memberPermsKeys = new[] 
        {
            Permissions.Projects.View, 
            Permissions.Boards.View,
            Permissions.Tasks.View,
            Permissions.Tasks.Create,
            Permissions.Tasks.Update,
            Permissions.Tasks.Assign,
            Permissions.Tasks.ChangeStatus,
            Permissions.Comments.View,
            Permissions.Comments.Create
        };
        foreach (var key in memberPermsKeys)
        {
            if(permissionMap.TryGetValue(key, out var p))
                memberRole.RolePermissions.Add(new RolePermission { RoleId = memberRole.Id, PermissionId = p.Id });
        }

        var viewerPermsKeys = allPermissions.Where(p => p.Key.EndsWith(".view")).Select(p => p.Key);
        foreach (var key in viewerPermsKeys)
        {
             if(permissionMap.TryGetValue(key, out var p))
                viewerRole.RolePermissions.Add(new RolePermission { RoleId = viewerRole.Id, PermissionId = p.Id });
        }

        await _context.SaveChangesAsync();
    }

    private async Task SeedUsersAsync(string defaultAdminPassword)
    {
        var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == Roles.Admin);
        if (adminRole == null) return;

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(defaultAdminPassword);

        var adminUser = new User
        {
            Username = "Admin",
            PasswordHash = passwordHash,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(adminUser);
        await _context.SaveChangesAsync();

        _context.UserRoles.Add(new UserRole { UserId = adminUser.Id, RoleId = adminRole.Id });
        await _context.SaveChangesAsync();
    }
}
