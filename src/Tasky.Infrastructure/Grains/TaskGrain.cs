using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tasky.Application.Interfaces;
using Tasky.Application.Interfaces.Grains;
using Tasky.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Tasky.Infrastructure.Grains;

public class TaskGrain : Grain, ITaskGrain
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TaskGrain> _logger;
    private readonly ISignalRNotifier _notifier;
    
    // In-memory state
    private TaskItem? _task;
    private bool _isInitialized;

    public TaskGrain(IServiceScopeFactory scopeFactory, ILogger<TaskGrain> logger, ISignalRNotifier notifier)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _notifier = notifier;
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        await LoadStateAsync();
        await base.OnActivateAsync(cancellationToken);
    }

    private async Task LoadStateAsync()
    {
        var taskId = (int)this.GetPrimaryKeyLong();
        
        using var scope = _scopeFactory.CreateScope();
        // Try load from Redis first? 
        // Logic: "Grains read-through from Redis -> DB fallback"
        // But since Grain IS the cache, maybe we just load from DB?
        // Requirement says: "Grains as read-through cache + coordinator".
        // "Use Redis for shared cache: ... grains read-through from Redis -> DB fallback".
        
        var redis = scope.ServiceProvider.GetRequiredService<IRedisService>();
        // Assuming we store Task in Redis. Key: task:{taskId}
        var task = await redis.GetAsync<TaskItem>($"task:{taskId}");
        
        if (task != null)
        {
             _task = task;
             _isInitialized = true;
             return;
        }
        
        // Fallback to DB
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        _task = await db.Tasks
            .Include(t => t.Comments)
            .Include(t => t.Assignee)
            .FirstOrDefaultAsync(t => t.Id == taskId);
            
        if (_task != null)
        {
            // Populate Redis
            await redis.SetAsync($"task:{taskId}", _task, TimeSpan.FromMinutes(30)); // TTL logic?
        }
        
        _isInitialized = true;
    }

    public async Task<TaskItem?> GetTaskAsync()
    {
        if (!_isInitialized) await LoadStateAsync();
        return _task;
    }

    public async Task OnTaskUpdatedAsync()
    {
        // Invalidate or reload
        await LoadStateAsync();
        // Notify SignalR? 
        // "TaskGrain notifies SignalR clients in task group task:{taskId}"
        // SignalR logic needs IHubContext.
        // But IHubContext is in Api. Grain is in Infrastructure.
        // We can use a SignalR Backplane (Redis) or expose an interface.
        // If Api is hosting Orleans, we can inject IHubContext if we reference Api/SignalR implementation?
        // No, Infrastructure shouldn't reference Api.
        // Interface ISignalRNotifier in Application?
    }

    public async Task OnCommentAddedAsync(TaskComment comment)
    {
        if (!_isInitialized) await LoadStateAsync();
        
        if (_task != null)
        {
            _task.Comments.Add(comment); // Append to in-memory state
            
            // Update Redis?
            using var scope = _scopeFactory.CreateScope();
            var redis = scope.ServiceProvider.GetRequiredService<IRedisService>();
            await redis.SetAsync($"task:{_task.Id}", _task);
            
            // Notify SignalR
            await _notifier.NotifyCommentAddedAsync(_task.Id, comment);
        }
    }
}
