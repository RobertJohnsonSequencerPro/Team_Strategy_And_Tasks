using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using TeamStrategyAndTasks.Core.Enums;
using TeamStrategyAndTasks.Infrastructure.Identity;
using TeamStrategyAndTasks.Web.Services;
using Xunit;

namespace TeamStrategyAndTasks.Web.Tests;

public class RoleBootstrapHelperTests
{
    [Fact]
    public async Task GetBootstrapRoleAsync_returns_administrator_when_no_admins_exist()
    {
        var userManager = BuildUserManagerMock(adminUsers: []);

        var role = await RoleBootstrapHelper.GetBootstrapRoleAsync(userManager.Object);

        role.Should().Be(nameof(UserRole.Administrator));
    }

    [Fact]
    public async Task GetBootstrapRoleAsync_returns_contributor_when_admin_exists()
    {
        var existingAdmin = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "admin@example.com",
            Email = "admin@example.com",
            DisplayName = "Admin"
        };
        var userManager = BuildUserManagerMock(adminUsers: [existingAdmin]);

        var role = await RoleBootstrapHelper.GetBootstrapRoleAsync(userManager.Object);

        role.Should().Be(nameof(UserRole.Contributor));
    }

    private static Mock<UserManager<ApplicationUser>> BuildUserManagerMock(IList<ApplicationUser> adminUsers)
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        var userManager = new Mock<UserManager<ApplicationUser>>(
            store.Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!);

        userManager
            .Setup(x => x.GetUsersInRoleAsync(nameof(UserRole.Administrator)))
            .ReturnsAsync(adminUsers);

        return userManager;
    }
}
