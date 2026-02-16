using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tasky.Application.Interfaces;
using Tasky.Application.Interfaces.Grains;
using Tasky.Domain.Entities;

namespace Tasky.Infrastructure.Grains;

public class UserGrain : Grain, IUserGrain
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<UserGrain> _logger;
    private User? _user;

    public UserGrain(IServiceScopeFactory scopeFactory, ILogger<UserGrain> logger)
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
        var userId = (int)this.GetPrimaryKeyLong();
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        
        _user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public Task<User?> GetUserAsync() => Task.FromResult(_user);

    public Task UpdateStatusAsync(string status)
    {
        _logger.LogInformation("User {UserId} status updated to {Status}", this.GetPrimaryKeyLong(), status);
        return Task.CompletedTask;
    }
}
