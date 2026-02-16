using System.Net.Http.Json;
using Tasky.Application.DTOs;

namespace Tasky.IntegrationTests;

public class AuthIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Should_Register_And_Login_Successfully()
    {
        // 1. Register
        var registerRequest = new RegisterRequest("testuser", "Passw0rd123!");
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var registerResult = await registerResponse.Content.ReadFromJsonAsync<RegisterResponse>();
        Assert.NotNull(registerResult?.UserId);

        // 2. Login
        var loginRequest = new LoginRequest("testuser", "Passw0rd123!");
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginResult?.Token);
        Assert.Equal("testuser", loginResult.User.Username);
    }
}
