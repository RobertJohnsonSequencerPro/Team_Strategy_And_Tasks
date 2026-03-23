namespace TeamStrategyAndTasks.Core.Interfaces;

public interface IUserService
{
    Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken ct = default);
    Task<UserDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<UserDto> UpdateUserAsync(Guid id, UpdateUserRequest request, CancellationToken ct = default);
    Task<UserDto> UpdateProfileAsync(Guid id, UpdateProfileRequest request, CancellationToken ct = default);
}
