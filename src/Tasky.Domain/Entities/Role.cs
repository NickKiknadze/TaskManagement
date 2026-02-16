using Tasky.Domain.Common;

namespace Tasky.Domain.Entities;

public class Role : BaseEntity
{
    public required string Name { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
