using Tasky.Application.DTOs;

namespace Tasky.Application.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
}

public interface IProjectService
{
    Task<int> CreateProjectAsync(CreateProjectRequest request, CancellationToken cancellationToken = default);
    Task<List<ProjectDto>> GetAllProjectsAsync(CancellationToken cancellationToken = default);
    Task<ProjectDetailDto?> GetProjectByIdAsync(int id, CancellationToken cancellationToken = default);
}

public interface IBoardService
{
    Task<int> CreateBoardAsync(CreateBoardRequest request, CancellationToken cancellationToken = default);
    Task<BoardDetailDto?> GetBoardByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateColumnAsync(CreateColumnRequest request, CancellationToken cancellationToken = default);
}

public interface ITaskService
{
    Task<int> CreateTaskAsync(CreateTaskRequest request, CancellationToken cancellationToken = default);
    Task UpdateTaskAsync(UpdateTaskRequest request, CancellationToken cancellationToken = default);
    Task<TaskDetailDto?> GetTaskByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> AddCommentAsync(AddCommentRequest request, CancellationToken cancellationToken = default);
}
