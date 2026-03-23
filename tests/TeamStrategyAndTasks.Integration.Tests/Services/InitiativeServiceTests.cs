using FluentAssertions;
using TeamStrategyAndTasks.Core.DTOs;
using TeamStrategyAndTasks.Core.Enums;
using TeamStrategyAndTasks.Core.Exceptions;
using TeamStrategyAndTasks.Infrastructure.Services;
using TeamStrategyAndTasks.Integration.Tests.Helpers;

namespace TeamStrategyAndTasks.Integration.Tests.Services;

public class InitiativeServiceTests
{
    private static InitiativeService CreateService(out Infrastructure.Data.AppDbContext db)
    {
        db = InMemoryDbHelper.CreateContext();
        return new InitiativeService(db);
    }

    [Fact]
    public async Task CreateAsync_WithValidRequest_ReturnsInitiativeWithAssignedId()
    {
        var service = CreateService(out _);
        var ownerId = Guid.NewGuid();
        var targetDate = DateTimeOffset.UtcNow.AddMonths(3);
        var request = new CreateInitiativeRequest("Test Initiative", "Description", targetDate);

        var result = await service.CreateAsync(request, ownerId);

        result.Id.Should().NotBeEmpty();
        result.Title.Should().Be("Test Initiative");
        result.Description.Should().Be("Description");
        result.OwnerId.Should().Be(ownerId);
        result.TargetDate.Should().Be(targetDate);
        result.Status.Should().Be(NodeStatus.NotStarted);
        result.IsArchived.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllAsync_ExcludesArchivedInitiatives()
    {
        var service = CreateService(out _);
        var ownerId = Guid.NewGuid();
        await service.CreateAsync(new CreateInitiativeRequest("Active Initiative", null, null), ownerId);
        var archived = await service.CreateAsync(new CreateInitiativeRequest("Archived Initiative", null, null), ownerId);
        await service.ArchiveAsync(archived.Id);

        var results = await service.GetAllAsync();

        results.Should().ContainSingle(i => i.Title == "Active Initiative");
        results.Should().NotContain(i => i.Title == "Archived Initiative");
    }

    [Fact]
    public async Task GetAllAsync_ReturnsInitiativesOrderedByTitle()
    {
        var service = CreateService(out _);
        var ownerId = Guid.NewGuid();
        await service.CreateAsync(new CreateInitiativeRequest("Zebra Initiative", null, null), ownerId);
        await service.CreateAsync(new CreateInitiativeRequest("Alpha Initiative", null, null), ownerId);

        var results = await service.GetAllAsync();

        results[0].Title.Should().Be("Alpha Initiative");
        results[1].Title.Should().Be("Zebra Initiative");
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsInitiative()
    {
        var service = CreateService(out _);
        var ownerId = Guid.NewGuid();
        var created = await service.CreateAsync(new CreateInitiativeRequest("My Initiative", null, null), ownerId);

        var result = await service.GetByIdAsync(created.Id);

        result.Id.Should().Be(created.Id);
        result.Title.Should().Be("My Initiative");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ThrowsNotFoundException()
    {
        var service = CreateService(out _);

        var act = () => service.GetByIdAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_WithValidRequest_UpdatesFields()
    {
        var service = CreateService(out _);
        var ownerId = Guid.NewGuid();
        var created = await service.CreateAsync(new CreateInitiativeRequest("Old Title", null, null), ownerId);
        var newTargetDate = DateTimeOffset.UtcNow.AddMonths(6);

        var updateRequest = new UpdateInitiativeRequest("New Title", "New Desc", newTargetDate, NodeStatus.InProgress);
        var result = await service.UpdateAsync(created.Id, updateRequest);

        result.Title.Should().Be("New Title");
        result.Description.Should().Be("New Desc");
        result.TargetDate.Should().Be(newTargetDate);
        result.Status.Should().Be(NodeStatus.InProgress);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistingId_ThrowsNotFoundException()
    {
        var service = CreateService(out _);
        var request = new UpdateInitiativeRequest("Title", null, null, NodeStatus.NotStarted);

        var act = () => service.UpdateAsync(Guid.NewGuid(), request);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ArchiveAsync_SetsIsArchivedTrue()
    {
        var service = CreateService(out var db);
        var ownerId = Guid.NewGuid();
        var created = await service.CreateAsync(new CreateInitiativeRequest("To Archive", null, null), ownerId);

        await service.ArchiveAsync(created.Id);

        var inDb = await db.Initiatives.FindAsync(created.Id);
        inDb!.IsArchived.Should().BeTrue();
    }

    [Fact]
    public async Task ArchiveAsync_WithNonExistingId_ThrowsNotFoundException()
    {
        var service = CreateService(out _);

        var act = () => service.ArchiveAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task LinkTaskAsync_CreatesInitiativeWorkTaskLink()
    {
        var service = CreateService(out var db);
        var initiativeId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        await service.LinkTaskAsync(initiativeId, taskId);

        var link = await db.InitiativeWorkTasks.FindAsync(initiativeId, taskId);
        link.Should().NotBeNull();
        link!.InitiativeId.Should().Be(initiativeId);
        link.WorkTaskId.Should().Be(taskId);
    }

    [Fact]
    public async Task LinkTaskAsync_WhenAlreadyLinked_DoesNotCreateDuplicate()
    {
        var service = CreateService(out var db);
        var initiativeId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        await service.LinkTaskAsync(initiativeId, taskId);
        await service.LinkTaskAsync(initiativeId, taskId);

        var links = db.InitiativeWorkTasks
            .Where(it => it.InitiativeId == initiativeId && it.WorkTaskId == taskId).ToList();
        links.Should().HaveCount(1);
    }

    [Fact]
    public async Task UnlinkTaskAsync_WithExistingLink_RemovesLink()
    {
        var service = CreateService(out var db);
        var initiativeId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        await service.LinkTaskAsync(initiativeId, taskId);
        await service.UnlinkTaskAsync(initiativeId, taskId);

        var link = db.InitiativeWorkTasks
            .FirstOrDefault(it => it.InitiativeId == initiativeId && it.WorkTaskId == taskId);
        link.Should().BeNull();
    }

    [Fact]
    public async Task UnlinkTaskAsync_WithNonExistingLink_DoesNotThrow()
    {
        var service = CreateService(out _);

        var act = () => service.UnlinkTaskAsync(Guid.NewGuid(), Guid.NewGuid());

        await act.Should().NotThrowAsync();
    }
}
