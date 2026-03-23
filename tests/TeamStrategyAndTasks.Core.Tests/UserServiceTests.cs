using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using TeamStrategyAndTasks.Core.DTOs;
using TeamStrategyAndTasks.Core.Exceptions;
using TeamStrategyAndTasks.Infrastructure.Identity;
using TeamStrategyAndTasks.Infrastructure.Services;

namespace TeamStrategyAndTasks.Core.Tests;

public class UserServiceTests
{
    private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var storeMock = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            storeMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    private static ApplicationUser MakeUser(string displayName = "Alice", string email = "alice@example.com") =>
        new()
        {
            Id = Guid.NewGuid(),
            DisplayName = displayName,
            Email = email,
            UserName = email,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

    [Fact]
    public async Task GetAllAsync_ReturnsAllUsersWithRoles()
    {
        // Arrange
        var user1 = MakeUser("Alice", "alice@example.com");
        var user2 = MakeUser("Bob", "bob@example.com");
        var userManagerMock = CreateUserManagerMock();

        userManagerMock.Setup(m => m.Users)
            .Returns(new[] { user1, user2 }.AsQueryable());
        userManagerMock.Setup(m => m.GetRolesAsync(user1))
            .ReturnsAsync(["Contributor"]);
        userManagerMock.Setup(m => m.GetRolesAsync(user2))
            .ReturnsAsync(["Administrator"]);

        var sut = new UserService(userManagerMock.Object);

        // Act
        var result = await sut.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(u => u.DisplayName == "Alice" && u.Roles.Contains("Contributor"));
        result.Should().Contain(u => u.DisplayName == "Bob" && u.Roles.Contains("Administrator"));
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsUser_WhenFound()
    {
        // Arrange
        var user = MakeUser();
        var userManagerMock = CreateUserManagerMock();
        userManagerMock.Setup(m => m.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);
        userManagerMock.Setup(m => m.GetRolesAsync(user))
            .ReturnsAsync(["Contributor"]);

        var sut = new UserService(userManagerMock.Object);

        // Act
        var result = await sut.GetByIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
        result.DisplayName.Should().Be(user.DisplayName);
        result.Roles.Should().Contain("Contributor");
    }

    [Fact]
    public async Task GetByIdAsync_Throws_WhenUserNotFound()
    {
        // Arrange
        var userManagerMock = CreateUserManagerMock();
        userManagerMock.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        var sut = new UserService(userManagerMock.Object);

        // Act
        var act = () => sut.GetByIdAsync(Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateUserAsync_UpdatesDisplayNameEmailAndRole()
    {
        // Arrange
        var user = MakeUser("Alice", "alice@example.com");
        var userManagerMock = CreateUserManagerMock();
        userManagerMock.Setup(m => m.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);
        userManagerMock.Setup(m => m.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);
        userManagerMock.Setup(m => m.GetRolesAsync(user))
            .ReturnsAsync(["Contributor"]);
        userManagerMock.Setup(m => m.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(IdentityResult.Success);
        userManagerMock.Setup(m => m.AddToRoleAsync(user, "StrategyOwner"))
            .ReturnsAsync(IdentityResult.Success);

        var sut = new UserService(userManagerMock.Object);
        var request = new UpdateUserRequest("Alice Updated", "alice@example.com", true, "StrategyOwner");

        // Act
        var result = await sut.UpdateUserAsync(user.Id, request);

        // Assert
        result.DisplayName.Should().Be("Alice Updated");
        userManagerMock.Verify(m => m.AddToRoleAsync(user, "StrategyOwner"), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_Throws_WhenUserManagerFails()
    {
        // Arrange
        var user = MakeUser();
        var userManagerMock = CreateUserManagerMock();
        userManagerMock.Setup(m => m.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);
        userManagerMock.Setup(m => m.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "DB error" }));

        var sut = new UserService(userManagerMock.Object);

        // Act
        var act = () => sut.UpdateUserAsync(user.Id, new UpdateUserRequest("X", "x@example.com", true, null));

        // Assert
        await act.Should().ThrowAsync<AppValidationException>();
    }

    [Fact]
    public async Task UpdateProfileAsync_UpdatesDisplayNameAndEmail()
    {
        // Arrange
        var user = MakeUser("Alice", "alice@example.com");
        var userManagerMock = CreateUserManagerMock();
        userManagerMock.Setup(m => m.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);
        userManagerMock.Setup(m => m.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);
        userManagerMock.Setup(m => m.GetRolesAsync(user))
            .ReturnsAsync(["Contributor"]);

        var sut = new UserService(userManagerMock.Object);

        // Act
        var result = await sut.UpdateProfileAsync(user.Id, new UpdateProfileRequest("Alice New", "alice_new@example.com"));

        // Assert
        result.Should().NotBeNull();
        user.DisplayName.Should().Be("Alice New");
        user.Email.Should().Be("alice_new@example.com");
        user.UserName.Should().Be("alice_new@example.com");
    }

    [Fact]
    public async Task UpdateProfileAsync_Throws_WhenUserNotFound()
    {
        // Arrange
        var userManagerMock = CreateUserManagerMock();
        userManagerMock.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        var sut = new UserService(userManagerMock.Object);

        // Act
        var act = () => sut.UpdateProfileAsync(Guid.NewGuid(), new UpdateProfileRequest("X", "x@x.com"));

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
