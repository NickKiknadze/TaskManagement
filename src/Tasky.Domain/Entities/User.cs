using Tasky.Domain.Common;

namespace Tasky.Domain.Entities;

public class User : BaseEntity
{
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public bool IsActive { get; set; } = true;
    
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
