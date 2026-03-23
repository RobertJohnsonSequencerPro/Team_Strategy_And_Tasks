using Microsoft.AspNetCore.Identity;
using TeamStrategyAndTasks.Core.DTOs;
using TeamStrategyAndTasks.Core.Exceptions;
using TeamStrategyAndTasks.Core.Interfaces;
using TeamStrategyAndTasks.Infrastructure.Identity;

namespace TeamStrategyAndTasks.Infrastructure.Services;

public class UserService(UserManager<ApplicationUser> userManager) : IUserService
{
    public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken ct = default)
    {
        var users = userManager.Users.OrderBy(u => u.DisplayName).ToList();
        var result = new List<UserDto>(users.Count);
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            result.Add(MapToDto(user, roles));
        }
        return result;
    }

    public async Task<UserDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(id.ToString())
            ?? throw new NotFoundException("User", id);
        var roles = await userManager.GetRolesAsync(user);
        return MapToDto(user, roles);
    }

    public async Task<UserDto> UpdateUserAsync(Guid id, UpdateUserRequest request, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(id.ToString())
            ?? throw new NotFoundException("User", id);

        user.DisplayName = string.IsNullOrWhiteSpace(request.DisplayName) ? user.DisplayName : request.DisplayName;
        user.IsActive = request.IsActive;

        if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != user.Email)
        {
            user.Email = request.Email;
            user.UserName = request.Email;
            user.NormalizedEmail = request.Email.ToUpperInvariant();
            user.NormalizedUserName = request.Email.ToUpperInvariant();
        }

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            throw new AppValidationException("User", string.Join(", ", updateResult.Errors.Select(e => e.Description)));

        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            var currentRoles = await userManager.GetRolesAsync(user);
            await userManager.RemoveFromRolesAsync(user, currentRoles);
            await userManager.AddToRoleAsync(user, request.Role);
        }

        var roles = await userManager.GetRolesAsync(user);
        return MapToDto(user, roles);
    }

    public async Task<UserDto> UpdateProfileAsync(Guid id, UpdateProfileRequest request, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(id.ToString())
            ?? throw new NotFoundException("User", id);

        user.DisplayName = string.IsNullOrWhiteSpace(request.DisplayName) ? user.DisplayName : request.DisplayName;

        if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != user.Email)
        {
            user.Email = request.Email;
            user.UserName = request.Email;
            user.NormalizedEmail = request.Email.ToUpperInvariant();
            user.NormalizedUserName = request.Email.ToUpperInvariant();
        }

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            throw new AppValidationException("User", string.Join(", ", updateResult.Errors.Select(e => e.Description)));

        var roles = await userManager.GetRolesAsync(user);
        return MapToDto(user, roles);
    }

    private static UserDto MapToDto(ApplicationUser user, IList<string> roles) =>
        new(user.Id, user.DisplayName, user.Email ?? string.Empty, user.IsActive, user.CreatedAt, roles.ToList());
}
