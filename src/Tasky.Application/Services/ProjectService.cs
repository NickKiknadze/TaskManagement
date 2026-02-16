using Microsoft.EntityFrameworkCore;
using Tasky.Application.DTOs;
using Tasky.Application.Interfaces;
using Tasky.Domain.Entities;

namespace Tasky.Application.Services;

public class ProjectService : IProjectService
{
    private readonly IApplicationDbContext _context;

    public ProjectService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> CreateProjectAsync(CreateProjectRequest request, CancellationToken cancellationToken = default)
    {
        var project = new Project
        {
            Name = request.Name,
            Description = request.Description
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync(cancellationToken);

        return project.Id;
    }

    public async Task<List<ProjectDto>> GetAllProjectsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .AsNoTracking()
            .Select(p => new ProjectDto(p.Id, p.Name, p.Description))
            .ToListAsync(cancellationToken);
    }

    public async Task<ProjectDetailDto?> GetProjectByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var project = await _context.Projects
            .Include(p => p.Boards)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (project == null) return null;

        return new ProjectDetailDto(
            project.Id,
            project.Name,
            project.Description,
            project.Boards.Select(b => new BoardDto(b.Id, b.Name)).ToList()
        );
    }
}
