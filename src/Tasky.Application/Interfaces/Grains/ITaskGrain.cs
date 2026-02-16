using Tasky.Domain.Entities;

namespace Tasky.Application.Interfaces.Grains;

public interface ITaskGrain : IGrainWithIntegerKey
{
    Task<TaskItem?> GetTaskAsync();
    
    // Called by API/Worker to notify of updates
    Task OnTaskUpdatedAsync(); 
    Task OnCommentAddedAsync(TaskComment comment);
}
