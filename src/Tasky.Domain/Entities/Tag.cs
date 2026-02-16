using Tasky.Domain.Common;

namespace Tasky.Domain.Entities;

public class Tag : BaseEntity
{
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    
    public required string Name { get; set; }
    public required string Color { get; set; }
    
    public ICollection<TaskTag> TaskTags { get; set; } = new List<TaskTag>();
}
