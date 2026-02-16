using Tasky.Domain.Common;
using Tasky.Domain.Enums;

namespace Tasky.Domain.Entities;

public class TaskItem : BaseEntity
{
    public int ColumnId { get; set; }
    public Column Column { get; set; } = null!;
    
    public required string Title { get; set; }
    public string? Description { get; set; }
    
    public Priority Priority { get; set; } = Priority.Medium;
    public DateTime? DueDate { get; set; }
    public string? Estimate { get; set; } // e.g. "2h", "1d"
    
    public int? AssigneeId { get; set; }
    public User? Assignee { get; set; }
    
    public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
    public ICollection<TaskTag> TaskTags { get; set; } = new List<TaskTag>();
}
