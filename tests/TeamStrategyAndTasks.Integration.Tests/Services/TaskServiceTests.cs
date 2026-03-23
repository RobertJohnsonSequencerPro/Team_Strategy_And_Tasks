using FluentAssertions;
using TeamStrategyAndTasks.Core.DTOs;
using TeamStrategyAndTasks.Core.Enums;
using TeamStrategyAndTasks.Core.Exceptions;
using TeamStrategyAndTasks.Infrastructure.Services;
using TeamStrategyAndTasks.Integration.Tests.Helpers;

namespace TeamStrategyAndTasks.Integration.Tests.Services;

public class TaskServiceTests
{
    private static TaskService CreateService(out Infrastructure.Data.AppDbContext db)
    {
        db = InMemoryDbHelper.CreateContext();
        return new TaskService(db);
    }

    [Fact]
    public async Task CreateAsync_WithValidRequest_ReturnsTaskWithAssignedId()
    {
        var service = CreateService(out _);
        var ownerId = Guid.NewGuid();
        var assigneeId = Guid.NewGuid();
        var targetDate = DateTimeOffset.UtcNow.AddDays(14);
        var request = new CreateTaskRequest("Test Task", "Description", assigneeId, 8.0m, targetDate);

        var result = await service.CreateAsync(request, ownerId);

        result.Id.Should().NotBeEmpty();
        result.Title.Should().Be("Test Task");
        result.Description.Should().Be("Description");
        result.OwnerId.Should().Be(ownerId);
        result.AssigneeId.Should().Be(assigneeId);
        result.EstimatedEffort.Should().Be(8.0m);
        result.TargetDate.Should().Be(targetDate);
        result.Status.Should().Be(NodeStatus.NotStarted);
        result.IsArchived.Should().BeFalse();
    }

    [Fact]
    public async Task CreateAsync_WithNullAssignee_CreatesTaskWithNullAssignee()
    {
        var service = CreateService(out _);
        var ownerId = Guid.NewGuid();
        var request = new CreateTaskRequest("Unassigned Task", null, null, null, null);

        var result = await service.CreateAsync(request, ownerId);

        result.AssigneeId.Should().BeNull();
        result.EstimatedEffort.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ExcludesArchivedTasks()
    {
        var service = CreateService(out _);
        var ownerId = Guid.NewGuid();
        await service.CreateAsync(new CreateTaskRequest("Active Task", null, null, null, null), ownerId);
        var archived = await service.CreateAsync(new CreateTaskRequest("Archived Task", null, null, null, null), ownerId);
        await service.ArchiveAsync(archived.Id);

        var results = await service.GetAllAsync();

        results.Should().ContainSingle(t => t.Title == "Active Task");
        results.Should().NotContain(t => t.Title == "Archived Task");
    }

    [Fact]
    public async Task GetAllAsync_ReturnsTasksOrderedByTitle()
    {
        var service = CreateService(out _);
        var ownerId = Guid.NewGuid();
        await service.CreateAsync(new CreateTaskRequest("Zebra Task", null, null, null, null), ownerId);
        await service.CreateAsync(new CreateTaskRequest("Alpha Task", null, null, null, null), ownerId);

        var results = await service.GetAllAsync();

        results[0].Title.Should().Be("Alpha Task");
        results[1].Title.Should().Be("Zebra Task");
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsTask()
    {
        var service = CreateService(out _);
        var ownerId = Guid.NewGuid();
        var created = await service.CreateAsync(new CreateTaskRequest("My Task", null, null, null, null), ownerId);

        var result = await service.GetByIdAsync(created.Id);

        result.Id.Should().Be(created.Id);
        result.Title.Should().Be("My Task");
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
        var created = await service.CreateAsync(new CreateTaskRequest("Old Title", null, null, null, null), ownerId);
        var newAssigneeId = Guid.NewGuid();

        var updateRequest = new UpdateTaskRequest(
            "New Title", "New Desc", newAssigneeId, 10.0m, 8.5m, null, NodeStatus.InProgress);
        var result = await service.UpdateAsync(created.Id, updateRequest);

        result.Title.Should().Be("New Title");
        result.Description.Should().Be("New Desc");
        result.AssigneeId.Should().Be(newAssigneeId);
        result.EstimatedEffort.Should().Be(10.0m);
        result.ActualEffort.Should().Be(8.5m);
        result.Status.Should().Be(NodeStatus.InProgress);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistingId_ThrowsNotFoundException()
    {
        var service = CreateService(out _);
        var request = new UpdateTaskRequest("Title", null, null, null, null, null, NodeStatus.NotStarted);

        var act = () => service.UpdateAsync(Guid.NewGuid(), request);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CompleteAsync_SetsStatusToDoneAndSetsCompletionDate()
    {
        var service = CreateService(out var db);
        var ownerId = Guid.NewGuid();
        var created = await service.CreateAsync(new CreateTaskRequest("To Complete", null, null, null, null), ownerId);

        var before = DateTimeOffset.UtcNow;
        await service.CompleteAsync(created.Id);
        var after = DateTimeOffset.UtcNow;

        var inDb = await db.WorkTasks.FindAsync(created.Id);
        inDb!.Status.Should().Be(NodeStatus.Done);
        inDb.CompletionDate.Should().NotBeNull();
        inDb.CompletionDate!.Value.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public async Task CompleteAsync_WithNonExistingId_ThrowsNotFoundException()
    {
        var service = CreateService(out _);

        var act = () => service.CompleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ArchiveAsync_SetsIsArchivedTrue()
    {
        var service = CreateService(out var db);
        var ownerId = Guid.NewGuid();
        var created = await service.CreateAsync(new CreateTaskRequest("To Archive", null, null, null, null), ownerId);

        await service.ArchiveAsync(created.Id);

        var inDb = await db.WorkTasks.FindAsync(created.Id);
        inDb!.IsArchived.Should().BeTrue();
    }

    [Fact]
    public async Task ArchiveAsync_WithNonExistingId_ThrowsNotFoundException()
    {
        var service = CreateService(out _);

        var act = () => service.ArchiveAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
