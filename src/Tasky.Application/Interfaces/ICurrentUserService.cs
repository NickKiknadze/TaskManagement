namespace Tasky.Application.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? Username { get; }
    IEnumerable<string> Roles { get; }
    IEnumerable<string> Permissions { get; }
}
