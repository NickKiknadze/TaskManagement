using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Tasky.Api.Hubs;

[Microsoft.AspNetCore.Authorization.Authorize]
public class TaskHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        // Add to user group?
        // Or client adds itself to task group via method
        await base.OnConnectedAsync();
    }

    public async Task JoinTaskGroup(int taskId)
    {
        // Check permission?
        // Requirements say "Frontend: Task detail page joins SignalR group task:{taskId}"
        // User must have "tasks.view" or similar.
        // We can check user.HasClaim("permissions", "tasks.view")
        
        await Groups.AddToGroupAsync(Context.ConnectionId, $"task:{taskId}");
    }

    public async Task LeaveTaskGroup(int taskId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"task:{taskId}");
    }
}
