using Tasky.Domain.Entities;

namespace Tasky.Application.Interfaces.Grains;

public interface IProjectGrain : IGrainWithIntegerKey
{
    Task<Project?> GetProjectAsync();
    Task OnTaskAddedAsync(int taskId);
}
