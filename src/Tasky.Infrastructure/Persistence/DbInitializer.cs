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
            /*
            if (_context.Database.IsSqlServer())
            {
                await _context.Database.MigrateAsync();
            }
            */
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
        // 1. Create permissions
        var allPermissions = Permissions.All().Select(p => new Permission
        {
            Key = p,
            Description = $"Permission to {p}"
        }).ToList();
        
        await _context.Permissions.AddRangeAsync(allPermissions);
        await _context.SaveChangesAsync();

        var permissionMap = await _context.Permissions.ToDictionaryAsync(p => p.Key, p => p);

        // 2. Create roles
        var adminRole = new Role { Name = Roles.Admin };
        var pmRole = new Role { Name = Roles.ProjectManager };
        var memberRole = new Role { Name = Roles.Member };
        var viewerRole = new Role { Name = Roles.Viewer };

        _context.Roles.AddRange(adminRole, pmRole, memberRole, viewerRole);
        await _context.SaveChangesAsync();

        // 3. Assign permissions to roles
        
        // Admin: All
        foreach (var p in allPermissions)
        {
            adminRole.RolePermissions.Add(new RolePermission { RoleId = adminRole.Id, PermissionId = p.Id });
        }

        // Project Manager: All except Users.Manage, Roles.Manage
        foreach (var p in allPermissions.Where(x => !x.Key.StartsWith("users.") && !x.Key.StartsWith("roles.")))
        {
            pmRole.RolePermissions.Add(new RolePermission { RoleId = pmRole.Id, PermissionId = p.Id });
        }
        // Check if PM should view users/roles? Req says: "except user/role manage". 
        // So view is allowed? "ProjectManager: all project/board/task/comment perms except user/role manage".
        // It doesn't explicitly say "view users", but usually PM needs to assign tasks to users, so they need to view users.
        // Let's add Users.View and Roles.View explicitly if missed above.
        // But "except user/role manage" implies `users.manage` and `roles.manage` are excluded.
        // The loop above excludes `users.*` and `roles.*`. So `users.view` is excluded.
        // Let's fix.
        
        var pmPermissions = allPermissions.Where(p => 
            !p.Key.Equals(Permissions.Users.Manage) && 
            !p.Key.Equals(Permissions.Roles.Manage)).ToList();
            
        // Wait, above loop was incorrect. I shouldn't rely on StartsWith for granular exclusion unless naming is perfect.
        // Let's clear role permissions and re-add correctly.
        pmRole.RolePermissions.Clear();
        foreach(var p in pmPermissions)
        {
             pmRole.RolePermissions.Add(new RolePermission { RoleId = pmRole.Id, PermissionId = p.Id });
        }

        // Member: view/create/update tasks, create/view comments, change status, assign self
        // Note: "assign self" is tricky if permission is just "tasks.assign". Usually means assign anyone.
        // Req: "Member: view/create/update tasks, create/view comments, change status, assign self"
        // Implies access to Tasks.* except maybe Delete?
        // "tasks.delete" is in All.
        var memberPermsKeys = new[] 
        {
            Permissions.Projects.View, // Needs to see projects to see tasks? Req says "projects.view" in list.
            Permissions.Boards.View,
            Permissions.Tasks.View,
            Permissions.Tasks.Create,
            Permissions.Tasks.Update,
            Permissions.Tasks.Assign,
            Permissions.Tasks.ChangeStatus,
            Permissions.Comments.View,
            Permissions.Comments.Create
        };
        // Req didn't explicitly say Member can view projects/boards but it's implied.
        // Let's stick to interpretation: Viewer has view-only. Member has + create/update tasks.
        
        foreach (var key in memberPermsKeys)
        {
            if(permissionMap.TryGetValue(key, out var p))
                memberRole.RolePermissions.Add(new RolePermission { RoleId = memberRole.Id, PermissionId = p.Id });
        }

        // Viewer: view-only
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

        // Hash password
        // Use BCrypt.Net.BCrypt.HashPassword
        // Requirement: "Passwords must be stored as BCrypt hashes only."
        // Using explicit work factor 11 as per example in prompt? 
        // Example hash: $2a$11$y9J8rFwDESrMfhKNAho/peEU2Xn0grSwO6ZIlvt2HHG9YAZt1XFtC
        // I will use BCrypt.Net.BCrypt.HashPassword(defaultAdminPassword) which uses default work factor (usually 10 or 11).
        
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
