using System.Net.Http.Headers;
using System.Net.Http.Json;
using Tasky.Application.DTOs;

namespace Tasky.IntegrationTests;

public class ProjectIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProjectIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task AuthenticateAsync()
    {
        // Login as Admin
        var loginRequest = new LoginRequest("Admin", "YourStrong@Passw0rd");
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        response.EnsureSuccessStatusCode();
        var loginResult = await response.Content.ReadFromJsonAsync<LoginResponse>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult?.Token);
    }

    [Fact]
    public async Task Should_Create_Project_Board_And_Column()
    {
        await AuthenticateAsync();

        // 1. Create Project
        var createProjectRequest = new CreateProjectRequest("Test Project", "Description");
        var projectResponse = await _client.PostAsJsonAsync("/api/projects", createProjectRequest);
        projectResponse.EnsureSuccessStatusCode();
        var projectId = await projectResponse.Content.ReadFromJsonAsync<int>();
        Assert.True(projectId > 0);

        // 2. Create Board
        var createBoardRequest = new CreateBoardRequest(projectId, "Test Board");
        var boardResponse = await _client.PostAsJsonAsync($"/api/projects/{projectId}/boards", createBoardRequest);
        boardResponse.EnsureSuccessStatusCode();
        var boardId = await boardResponse.Content.ReadFromJsonAsync<int>();
        Assert.True(boardId > 0);

        // 3. Create Column
        var createColumnRequest = new CreateColumnRequest(boardId, "To Do", 1);
        var columnResponse = await _client.PostAsJsonAsync($"/api/boards/{boardId}/columns", createColumnRequest);
        columnResponse.EnsureSuccessStatusCode();
        var columnId = await columnResponse.Content.ReadFromJsonAsync<int>();
        Assert.True(columnId > 0);
        
        // 4. Get Board Detail
        var detailResponse = await _client.GetAsync($"/api/boards/{boardId}");
        detailResponse.EnsureSuccessStatusCode();
        var detail = await detailResponse.Content.ReadFromJsonAsync<BoardDetailDto>();
        Assert.NotNull(detail);
        Assert.Single(detail.Columns);
        Assert.Equal("To Do", detail.Columns[0].Name);
    }
}
