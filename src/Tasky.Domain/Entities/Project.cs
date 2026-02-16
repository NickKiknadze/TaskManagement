using Tasky.Domain.Common;

namespace Tasky.Domain.Entities;

public class Project : BaseEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    

    
    public ICollection<Board> Boards { get; set; } = new List<Board>();
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
