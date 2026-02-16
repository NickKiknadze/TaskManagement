using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tasky.Application.DTOs;
using Tasky.Application.Services;

namespace Tasky.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BoardsController : ControllerBase
{
    private readonly IBoardService _boardService;

    public BoardsController(IBoardService boardService)
    {
        _boardService = boardService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BoardDetailDto>> GetById(int id)
    {
        var board = await _boardService.GetBoardByIdAsync(id);
        if (board == null) return NotFound();
        return Ok(board);
    }

    [HttpPost("{boardId}/columns")]
    public async Task<ActionResult<int>> AddColumn(int boardId, [FromBody] CreateColumnRequest request)
    {
        if (boardId != request.BoardId) return BadRequest("Board ID mismatch");
        var columnId = await _boardService.CreateColumnAsync(request);
        return Ok(columnId);
    }
}
