using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tasky.Application.DTOs;
using Tasky.Application.Services;

namespace Tasky.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create([FromBody] CreateTaskRequest request)
    {
        var taskId = await _taskService.CreateTaskAsync(request);
        return Ok(taskId);
    }

    [HttpPost("{taskId}/comments")]
    public async Task<ActionResult<int>> AddComment(int taskId, [FromBody] AddCommentRequest request)
    {
        if (taskId != request.TaskId) return BadRequest("Task ID mismatch");
        var commentId = await _taskService.AddCommentAsync(request);
        return Ok(commentId);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskDetailDto>> GetById(int id)
    {
        var task = await _taskService.GetTaskByIdAsync(id);
        if (task == null) return NotFound();
        return Ok(task);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateTaskRequest request)
    {
        if (id != request.TaskId) return BadRequest("Task ID mismatch");
        await _taskService.UpdateTaskAsync(request);
        return NoContent();
    }
}
