using Tasky.Domain.Enums;

namespace Tasky.Application.DTOs;

public record CreateTaskRequest(int ColumnId, string Title, string Description, Priority Priority, int? AssigneeId, DateTime? DueDate, string? Estimate);
public record UpdateTaskRequest(int TaskId, string? Title, string? Description, Priority? Priority, int? AssigneeId, int? ColumnId, DateTime? DueDate);
public record AddCommentRequest(int TaskId, string Text);
public record CommentDto(int Id, string Text, string AuthorName, DateTime CreatedAt);

public record TaskDetailDto(
    int Id, 
    int ColumnId, 
    string Title, 
    string Description, 
    string Priority, 
    int? AssigneeId, 
    string? AssigneeName,
    DateTime? DueDate,
    string? Estimate,
    List<CommentDto> Comments
);
