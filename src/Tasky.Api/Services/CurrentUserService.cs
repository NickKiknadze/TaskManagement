using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Tasky.Application.Interfaces;

namespace Tasky.Api.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    public string? Username => _httpContextAccessor.HttpContext?.User?.Identity?.Name;
    
    public IEnumerable<string> Roles => _httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role).Select(c => c.Value) ?? Enumerable.Empty<string>();
    
    public IEnumerable<string> Permissions => _httpContextAccessor.HttpContext?.User?.FindAll("permissions").Select(c => c.Value) ?? Enumerable.Empty<string>();
}
