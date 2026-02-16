using Microsoft.EntityFrameworkCore;
using Tasky.Domain.Entities;

namespace Tasky.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<RolePermission> RolePermissions { get; }
    
    DbSet<Project> Projects { get; }
    DbSet<Board> Boards { get; }
    DbSet<Column> Columns { get; }
    DbSet<TaskItem> Tasks { get; }
    DbSet<TaskComment> TaskComments { get; }
    DbSet<Tag> Tags { get; }
    DbSet<TaskTag> TaskTags { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
