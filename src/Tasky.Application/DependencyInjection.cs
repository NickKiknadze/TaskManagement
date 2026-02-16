using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using FluentValidation;
using Tasky.Application.Services;

namespace Tasky.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IBoardService, BoardService>();
        services.AddScoped<ITaskService, TaskService>();
        
        return services;
    }
}
