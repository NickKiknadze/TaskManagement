namespace Tasky.Application.DTOs;

public record CreateProjectRequest(string Name, string Description);
public record ProjectDto(int Id, string Name, string Description);
public record ProjectDetailDto(int Id, string Name, string Description, List<BoardDto> Boards);
public record BoardDto(int Id, string Name);
