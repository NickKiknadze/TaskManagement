using Microsoft.EntityFrameworkCore;
using Tasky.Application.DTOs;
using Tasky.Application.Interfaces;
using Tasky.Domain.Entities;

namespace Tasky.Application.Services;

public class BoardService : IBoardService
{
    private readonly IApplicationDbContext _context;

    public BoardService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> CreateBoardAsync(CreateBoardRequest request, CancellationToken cancellationToken = default)
    {
        var project = await _context.Projects.FindAsync(new object[] { request.ProjectId }, cancellationToken);
        if (project == null)
        {
            throw new KeyNotFoundException($"Project with ID {request.ProjectId} not found.");
        }

        var board = new Board
        {
            ProjectId = request.ProjectId,
            Name = request.Name
        };

        _context.Boards.Add(board);
        await _context.SaveChangesAsync(cancellationToken);

        return board.Id;
    }

    public async Task<BoardDetailDto?> GetBoardByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var board = await _context.Boards
            .Include(b => b.Columns)
            .ThenInclude(c => c.Tasks)
            .ThenInclude(t => t.Assignee)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        if (board == null) return null;

        var columns = board.Columns.OrderBy(c => c.Order).Select(c => new ColumnDto(
            c.Id, 
            c.Name, 
            c.Order,
            c.Tasks.Select(t => new TaskDto(t.Id, t.Title, t.Description, t.Priority.ToString(), t.AssigneeId, t.Assignee?.Username)).ToList()
        )).ToList();

        return new BoardDetailDto(board.Id, board.ProjectId, board.Name, columns);
    }

    public async Task<int> CreateColumnAsync(CreateColumnRequest request, CancellationToken cancellationToken = default)
    {
        var col = new Column
        {
            BoardId = request.BoardId,
            Name = request.Name,
            Order = request.Order,
            Tasks = new List<TaskItem>()
        };

        _context.Columns.Add(col);
        await _context.SaveChangesAsync(cancellationToken);

        return col.Id;
    }
}
