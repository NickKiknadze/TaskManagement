using Microsoft.AspNetCore.SignalR;
using Tasky.Api.Hubs;
using Tasky.Application.Interfaces;
using Tasky.Domain.Entities;

namespace Tasky.Api.Services;

public class SignalRNotifier : ISignalRNotifier
{
    private readonly IHubContext<TaskHub> _hubContext;

    public SignalRNotifier(IHubContext<TaskHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyTaskUpdatedAsync(int taskId)
    {
        await _hubContext.Clients.Group($"task:{taskId}").SendAsync("TaskUpdated", taskId);
    }

    public async Task NotifyCommentAddedAsync(int taskId, TaskComment comment)
    {
        // We might want to send a DTO, but for now send entity or anonymous object
        // Avoid sending full entity with cycles if serializer handles it badly, but System.Text.Json usually errors on cycles.
        // Configure SignalR JSON options or map to DTO.
        // For simplicity, let's map to anonymous object or just send necessary fields.
        var dto = new 
        {
            Id = comment.Id,
            TaskId = comment.TaskId,
            AuthorId = comment.AuthorId,
            Text = comment.Text,
            CreatedAt = comment.CreatedAt
        };
        await _hubContext.Clients.Group($"task:{taskId}").SendAsync("CommentAdded", dto);
    }
}
