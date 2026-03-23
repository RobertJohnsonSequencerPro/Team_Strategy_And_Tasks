namespace TeamStrategyAndTasks.Core.DTOs;

public record UserDto(
    Guid Id,
    string DisplayName,
    string Email,
    bool IsActive,
    DateTimeOffset CreatedAt,
    IReadOnlyList<string> Roles);

public record UpdateUserRequest(
    string DisplayName,
    string Email,
    bool IsActive,
    string? Role);

public record UpdateProfileRequest(
    string DisplayName,
    string Email);
