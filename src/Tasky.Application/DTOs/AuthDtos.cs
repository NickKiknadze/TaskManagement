namespace Tasky.Application.DTOs;

public record LoginRequest(string Username, string Password);
public record LoginResponse(string Token, string RefreshToken, UserDto User);
public record RegisterRequest(string Username, string Password);
public record RegisterResponse(string UserId);
public record UserDto(string Id, string Username, IEnumerable<string> Roles, IEnumerable<string> Permissions);
