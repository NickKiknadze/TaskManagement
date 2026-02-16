using Microsoft.EntityFrameworkCore;
using Tasky.Application.DTOs;
using Tasky.Application.Interfaces;
using Tasky.Application.Interfaces.Grains;
using Tasky.Domain.Entities;

namespace Tasky.Application.Services;

public class TaskService : ITaskService
{
    private readonly IApplicationDbContext _context;
    private readonly IKafkaProducer _kafkaProducer;
    private readonly ICurrentUserService _currentUser;
    private readonly IClusterClient _orleansClient;

    public TaskService(IApplicationDbContext context, IKafkaProducer kafkaProducer, ICurrentUserService currentUser, IClusterClient orleansClient)
    {
        _context = context;
        _kafkaProducer = kafkaProducer;
        _currentUser = currentUser;
        _orleansClient = orleansClient;
    }

    public async Task<int> CreateTaskAsync(CreateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var task = new TaskItem
        {
            ColumnId = request.ColumnId,
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            AssigneeId = request.AssigneeId,
            DueDate = request.DueDate,
            Estimate = request.Estimate
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync(cancellationToken);

        return task.Id;
    }

    public async Task UpdateTaskAsync(UpdateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var task = await _context.Tasks.FindAsync(new object[] { request.TaskId }, cancellationToken);
        if (task == null) throw new KeyNotFoundException("Task not found");

        if (request.Title != null) task.Title = request.Title;
        if (request.Description != null) task.Description = request.Description;
        if (request.Priority.HasValue) task.Priority = request.Priority.Value;
        if (request.AssigneeId.HasValue) task.AssigneeId = request.AssigneeId.Value > 0 ? request.AssigneeId.Value : null;
        if (request.ColumnId.HasValue) task.ColumnId = request.ColumnId.Value;
        if (request.DueDate.HasValue) task.DueDate = request.DueDate.Value;

        await _context.SaveChangesAsync(cancellationToken);

        var grain = _orleansClient.GetGrain<ITaskGrain>(task.Id);
        await grain.OnTaskUpdatedAsync();
    }

    public async Task<TaskDetailDto?> GetTaskByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var task = await _context.Tasks
            .Include(t => t.Assignee)
            .Include(t => t.Comments)
            .ThenInclude(c => c.Author)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (task == null) return null;

        return new TaskDetailDto(
            task.Id,
            task.ColumnId,
            task.Title,
            task.Description,
            task.Priority.ToString(),
            task.AssigneeId,
            task.Assignee?.Username,
            task.DueDate,
            task.Estimate,
            task.Comments.OrderBy(c => c.CreatedAt).Select(c => new CommentDto(c.Id, c.Text, c.Author.Username, c.CreatedAt)).ToList()
        );
    }

    public async Task<int> AddCommentAsync(AddCommentRequest request, CancellationToken cancellationToken = default)
    {
        var task = await _context.Tasks.FindAsync(new object[] { request.TaskId }, cancellationToken);
        if (task == null) throw new KeyNotFoundException("Task not found");
        
        var userIdStr = _currentUser.UserId;
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
        {
             userId = 0; 
        }

        var comment = new TaskComment
        {
            TaskId = request.TaskId,
            Text = request.Text,
            AuthorId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.TaskComments.Add(comment);
        await _context.SaveChangesAsync(cancellationToken);
        
        await _kafkaProducer.ProduceAsync("task.comment.created", comment.Id.ToString(), comment);

        return comment.Id;
    }
}
