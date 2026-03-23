using FluentAssertions;
using TeamStrategyAndTasks.Core.DTOs;
using TeamStrategyAndTasks.Core.Enums;
using TeamStrategyAndTasks.Core.Exceptions;
using TeamStrategyAndTasks.Infrastructure.Services;
using TeamStrategyAndTasks.Integration.Tests.Helpers;

namespace TeamStrategyAndTasks.Integration.Tests.Services;

public class ObjectiveServiceTests
{
    private static ObjectiveService CreateService(out Infrastructure.Data.AppDbContext db)
    {
        db = InMemoryDbHelper.CreateContext();
        return new ObjectiveService(db);
    }

    [Fact]
    public async Task CreateAsync_WithValidRequest_ReturnsObjectiveWithAssignedId()
    {
        var service = CreateService(out _);
        var ownerId = Guid.NewGuid();
        var request = new CreateObjectiveRequest("Test Objective", "Description", "Metric", "100%", null);

        var result = await service.CreateAsync(request, ownerId);

        result.Id.Should().NotBeEmpty();
        result.Title.Should().Be("Test Objective");
        result.Description.Should().Be("Description");
        result.OwnerId.Should().Be(ownerId);
        result.SuccessMetric.Should().Be("Metric");
        result.TargetValue.Should().Be("100%");
        result.Status.Should().Be(NodeStatus.NotStarted);
        result.IsArchived.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllAsync_ExcludesArchivedObjectives()
    {
        var service = CreateService(out var db);
        var ownerId = Guid.NewGuid();

        var active = await service.CreateAsync(new CreateObjectiveRequest("Active", null, null, null, null), ownerId);
        var archived = await service.CreateAsync(new CreateObjectiveRequest("Archived", null, null, null, null), ownerId);
        await service.ArchiveAsync(archived.Id);

        var results = await service.GetAllAsync();

        results.Should().ContainSingle(o => o.Id == active.Id);
        results.Should().NotContain(o => o.Id == archived.Id);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsObjectivesOrderedByTitle()
    {
        var service = CreateService(out _);
        var ownerId = Guid.NewGuid();
        await service.CreateAsync(new CreateObjectiveRequest("Zebra Objective", null, null, null, null), ownerId);
        await service.CreateAsync(new CreateObjectiveRequest("Alpha Objective", null, null, null, null), ownerId);

        var results = await service.GetAllAsync();

        results[0].Title.Should().Be("Alpha Objective");
        results[1].Title.Should().Be("Zebra Objective");
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsObjective()
    {
        var service = CreateService(out _);
        var ownerId = Guid.NewGuid();
        var created = await service.CreateAsync(new CreateObjectiveRequest("My Objective", null, null, null, null), ownerId);

        var result = await service.GetByIdAsync(created.Id);

        result.Id.Should().Be(created.Id);
        result.Title.Should().Be("My Objective");
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
        var created = await service.CreateAsync(new CreateObjectiveRequest("Old Title", null, null, null, null), ownerId);

        var updateRequest = new UpdateObjectiveRequest("New Title", "New Desc", "New Metric", "200%", null, NodeStatus.InProgress);
        var result = await service.UpdateAsync(created.Id, updateRequest);

        result.Title.Should().Be("New Title");
        result.Description.Should().Be("New Desc");
        result.SuccessMetric.Should().Be("New Metric");
        result.TargetValue.Should().Be("200%");
        result.Status.Should().Be(NodeStatus.InProgress);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistingId_ThrowsNotFoundException()
    {
        var service = CreateService(out _);
        var request = new UpdateObjectiveRequest("Title", null, null, null, null, NodeStatus.NotStarted);

        var act = () => service.UpdateAsync(Guid.NewGuid(), request);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ArchiveAsync_SetsIsArchivedTrue()
    {
        var service = CreateService(out var db);
        var ownerId = Guid.NewGuid();
        var created = await service.CreateAsync(new CreateObjectiveRequest("To Archive", null, null, null, null), ownerId);

        await service.ArchiveAsync(created.Id);

        var inDb = await db.Objectives.FindAsync(created.Id);
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
    public async Task LinkProcessAsync_CreatesObjectiveProcessLink()
    {
        var service = CreateService(out var db);
        var objectiveId = Guid.NewGuid();
        var processId = Guid.NewGuid();

        await service.LinkProcessAsync(objectiveId, processId);

        var link = await db.ObjectiveProcesses.FindAsync(objectiveId, processId);
        link.Should().NotBeNull();
        link!.ObjectiveId.Should().Be(objectiveId);
        link.ProcessId.Should().Be(processId);
    }

    [Fact]
    public async Task LinkProcessAsync_WhenAlreadyLinked_DoesNotCreateDuplicate()
    {
        var service = CreateService(out var db);
        var objectiveId = Guid.NewGuid();
        var processId = Guid.NewGuid();

        await service.LinkProcessAsync(objectiveId, processId);
        await service.LinkProcessAsync(objectiveId, processId); // Second call should be idempotent

        var links = db.ObjectiveProcesses.Where(op => op.ObjectiveId == objectiveId && op.ProcessId == processId).ToList();
        links.Should().HaveCount(1);
    }

    [Fact]
    public async Task UnlinkProcessAsync_WithExistingLink_RemovesLink()
    {
        var service = CreateService(out var db);
        var objectiveId = Guid.NewGuid();
        var processId = Guid.NewGuid();

        await service.LinkProcessAsync(objectiveId, processId);
        await service.UnlinkProcessAsync(objectiveId, processId);

        var link = db.ObjectiveProcesses.FirstOrDefault(op => op.ObjectiveId == objectiveId && op.ProcessId == processId);
        link.Should().BeNull();
    }

    [Fact]
    public async Task UnlinkProcessAsync_WithNonExistingLink_DoesNotThrow()
    {
        var service = CreateService(out _);

        var act = () => service.UnlinkProcessAsync(Guid.NewGuid(), Guid.NewGuid());

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetFullHierarchyAsync_ExcludesArchivedObjectives()
    {
        var service = CreateService(out _);
        var ownerId = Guid.NewGuid();
        await service.CreateAsync(new CreateObjectiveRequest("Active", null, null, null, null), ownerId);
        var archived = await service.CreateAsync(new CreateObjectiveRequest("Archived", null, null, null, null), ownerId);
        await service.ArchiveAsync(archived.Id);

        var results = await service.GetFullHierarchyAsync();

        results.Should().ContainSingle();
        results[0].Title.Should().Be("Active");
    }
}
