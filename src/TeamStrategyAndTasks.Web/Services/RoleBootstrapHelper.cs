using Microsoft.AspNetCore.Identity;
using TeamStrategyAndTasks.Core.Enums;
using TeamStrategyAndTasks.Infrastructure.Identity;

namespace TeamStrategyAndTasks.Web.Services;

public static class RoleBootstrapHelper
{
    public static async Task<string> GetBootstrapRoleAsync(UserManager<ApplicationUser> userManager)
    {
        var admins = await userManager.GetUsersInRoleAsync(nameof(UserRole.Administrator));
        return admins.Count == 0 ? nameof(UserRole.Administrator) : nameof(UserRole.Contributor);
    }
}
