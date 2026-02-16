using Tasky.Domain.Common;

namespace Tasky.Domain.Entities;

public class Column : BaseEntity
{
    public int BoardId { get; set; }
    public Board Board { get; set; } = null!;
    
    public required string Name { get; set; }
    public int Order { get; set; }
    
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
