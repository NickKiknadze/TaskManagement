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
        var redis = scope.ServiceProvider.GetRequiredService<IRedisService>();
        var task = await redis.GetAsync<TaskItem>($"task:{taskId}");
        
        if (task != null)
        {
             _task = task;
             _isInitialized = true;
             return;
        }
        
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        _task = await db.Tasks
            .Include(t => t.Comments)
            .Include(t => t.Assignee)
            .FirstOrDefaultAsync(t => t.Id == taskId);
            
        if (_task != null)
        {
            await redis.SetAsync($"task:{taskId}", _task, TimeSpan.FromMinutes(30)); 
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
        await LoadStateAsync();
    }

    public async Task OnCommentAddedAsync(TaskComment comment)
    {
        if (!_isInitialized) await LoadStateAsync();
        
        if (_task != null)
        {
            _task.Comments.Add(comment); 
            
            using var scope = _scopeFactory.CreateScope();
            var redis = scope.ServiceProvider.GetRequiredService<IRedisService>();
            await redis.SetAsync($"task:{_task.Id}", _task);
            
            await _notifier.NotifyCommentAddedAsync(_task.Id, comment);
        }
    }
}
