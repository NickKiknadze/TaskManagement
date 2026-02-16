using Tasky.Domain.Common;

namespace Tasky.Domain.Entities;

public class Project : BaseEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    
    // For simplicity, we can have an Owner, or use RBAC for project access.
    // Requirement says "Role-based permissions (RBAC) with role-permission mapping"
    // But usually we need to know who created it or who owns it.
    
    public ICollection<Board> Boards { get; set; } = new List<Board>();
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
