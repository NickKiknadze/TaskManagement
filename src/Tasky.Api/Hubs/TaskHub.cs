using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Tasky.Api.Hubs;

[Microsoft.AspNetCore.Authorization.Authorize]
public class TaskHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public async Task JoinTaskGroup(int taskId)
    {
        
        await Groups.AddToGroupAsync(Context.ConnectionId, $"task:{taskId}");
    }

    public async Task LeaveTaskGroup(int taskId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"task:{taskId}");
    }
}
