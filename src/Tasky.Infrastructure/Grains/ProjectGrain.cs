using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tasky.Application.Interfaces;
using Tasky.Application.Interfaces.Grains;
using Tasky.Domain.Entities;

namespace Tasky.Infrastructure.Grains;

public class ProjectGrain : Grain, IProjectGrain
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ProjectGrain> _logger;
    private Project? _project;

    public ProjectGrain(IServiceScopeFactory scopeFactory, ILogger<ProjectGrain> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        await LoadStateAsync();
        await base.OnActivateAsync(cancellationToken);
    }

    private async Task LoadStateAsync()
    {
        var projectId = (int)this.GetPrimaryKeyLong();
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        
        _project = await context.Projects
            .Include(p => p.Boards)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == projectId);
    }

    public Task<Project?> GetProjectAsync() => Task.FromResult(_project);

    public Task OnTaskAddedAsync(int taskId)
    {
        _logger.LogInformation("Task {TaskId} added to Project {ProjectId}", taskId, this.GetPrimaryKeyLong());
        return Task.CompletedTask;
    }
}
