using Tasky.Domain.Common;

namespace Tasky.Domain.Entities;

public class Permission : BaseEntity
{
    public required string Key { get; set; } // e.g. "projects.create"
    public required string Description { get; set; }
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
