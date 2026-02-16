using Tasky.Domain.Common;

namespace Tasky.Domain.Entities;

public class Board : BaseEntity
{
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    
    public required string Name { get; set; }
    
    public ICollection<Column> Columns { get; set; } = new List<Column>();
}
