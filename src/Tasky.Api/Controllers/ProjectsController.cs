using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tasky.Application.DTOs;
using Tasky.Application.Services;
using Tasky.Domain.Constants;

namespace Tasky.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;
    private readonly IBoardService _boardService; // For CreateBoard

    public ProjectsController(IProjectService projectService, IBoardService boardService)
    {
        _projectService = projectService;
        _boardService = boardService;
    }

    [HttpPost]
    [Authorize(Policy = Permissions.Projects.Create)]
    public async Task<ActionResult<int>> Create([FromBody] CreateProjectRequest request)
    {
        var projectId = await _projectService.CreateProjectAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = projectId }, projectId);
    }

    [HttpGet]
    [Authorize(Policy = Permissions.Projects.View)]
    public async Task<ActionResult<List<ProjectDto>>> GetAll()
    {
        return Ok(await _projectService.GetAllProjectsAsync());
    }

    [HttpGet("{id}")]
    [Authorize(Policy = Permissions.Projects.View)]
    public async Task<ActionResult<ProjectDetailDto>> GetById(int id)
    {
        var project = await _projectService.GetProjectByIdAsync(id);
        if (project == null) return NotFound();
        return Ok(project);
    }

    [HttpPost("{projectId}/boards")]
    [Authorize(Policy = Permissions.Projects.Update)]
    public async Task<ActionResult<int>> CreateBoard(int projectId, [FromBody] CreateBoardRequest request)
    {
        if (projectId != request.ProjectId) return BadRequest("Project ID mismatch");
        var boardId = await _boardService.CreateBoardAsync(request);
        return CreatedAtAction("GetById", "Boards", new { id = boardId }, boardId);
    }
}
