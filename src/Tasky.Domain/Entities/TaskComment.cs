using Tasky.Domain.Common;

namespace Tasky.Domain.Entities;

public class TaskComment : BaseEntity
{
    public int TaskId { get; set; }
    public TaskItem Task { get; set; } = null!;
    
    public int AuthorId { get; set; }
    public User Author { get; set; } = null!;
    
    public required string Text { get; set; }
}
