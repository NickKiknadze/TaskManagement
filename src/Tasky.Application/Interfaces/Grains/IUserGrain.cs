using Tasky.Domain.Entities;

namespace Tasky.Application.Interfaces.Grains;

public interface IUserGrain : IGrainWithIntegerKey
{
    Task<User?> GetUserAsync();
    Task UpdateStatusAsync(string status);
}
