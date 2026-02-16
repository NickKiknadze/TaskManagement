namespace Tasky.Application.DTOs;

public record CreateBoardRequest(int ProjectId, string Name);
public record BoardDetailDto(int Id, int ProjectId, string Name, List<ColumnDto> Columns);
public record ColumnDto(int Id, string Name, int Order, List<TaskDto> Tasks);
public record TaskDto(int Id, string Title, string Description, string Priority, int? AssigneeId, string? AssigneeName);

public record CreateColumnRequest(int BoardId, string Name, int Order);
