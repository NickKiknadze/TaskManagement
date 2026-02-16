using Tasky.Domain.Entities;

namespace Tasky.Application.Interfaces;

public interface ISignalRNotifier
{
    Task NotifyTaskUpdatedAsync(int taskId);
    Task NotifyCommentAddedAsync(int taskId, TaskComment comment);
}
