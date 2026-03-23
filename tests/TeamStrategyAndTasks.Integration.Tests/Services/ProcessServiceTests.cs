using FluentAssertions;
using TeamStrategyAndTasks.Core.DTOs;
using TeamStrategyAndTasks.Core.Enums;
using TeamStrategyAndTasks.Core.Exceptions;
using TeamStrategyAndTasks.Infrastructure.Services;
using TeamStrategyAndTasks.Integration.Tests.Helpers;

namespace TeamStrategyAndTasks.Integration.Tests.Services;

public class ProcessServiceTests
{
    private static ProcessService CreateService(out Infrastructure.Data.AppDbContext db)
    {
        db = InMemoryDbHelper.CreateContext();
        return new ProcessService(db);
    }

    [Fact]
    public async Task CreateAsync_WithValidRequest_ReturnsProcessWithAssignedId()
    {
        var service = CreateService(out _);
        var ownerId = Guid.NewGuid();
        var request = new CreateProcessRequest("Test Process", "Description", "Metric", "Target", null);

        var result = await service.CreateAsync(request, ownerId);

        result.Id.Should().NotBeEmpty();
        result.Title.Should().Be("Test Process");
        result.Description.Should().Be("Description");
        result.OwnerId.Should().Be(ownerId);
        result.Status.Should().Be(NodeStatus.NotStarted);
        result.IsArchived.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllAsync_ExcludesArchivedProcesses()
    {
        var service = CreateService(out _);
        var ownerId = Guid.NewGuid();
        await service.CreateAsync(new CreateProcessRequest("Active Process", null, null, null, null), ownerId);
        var archived = await service.CreateAsync(new CreateProcessRequest("Archived Process", null, null, null, null), ownerId);
        await service.ArchiveAsync(archived.Id);

        var results = await service.GetAllAsync();

        results.Should().ContainSingle(p => p.Title == "Active Process");
        results.Should().NotContain(p => p.Title == "Archived Process");
    }

    [Fact]
    public async Task GetAllAsync_ReturnsProcessesOrderedByTitle()
    {
        var service = CreateService(out _);
        var ownerId = Guid.NewGuid();
        await service.CreateAsync(new CreateProcessRequest("Zebra Process", null, null, null, null), ownerId);
        await service.CreateAsync(new CreateProcessRequest("Alpha Process", null, null, null, null), ownerId);

        var results = await service.GetAllAsync();

        results[0].Title.Should().Be("Alpha Process");
        results[1].Title.Should().Be("Zebra Process");
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsProcess()
    {
        var service = CreateService(out _);
        var ownerId = Guid.NewGuid();
        var created = await service.CreateAsync(new CreateProcessRequest("My Process", null, null, null, null), ownerId);

        var result = await service.GetByIdAsync(created.Id);

        result.Id.Should().Be(created.Id);
        result.Title.Should().Be("My Process");
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
        var created = await service.CreateAsync(new CreateProcessRequest("Old Title", null, null, null, null), ownerId);

        var updateRequest = new UpdateProcessRequest("New Title", "New Desc", "New Metric", "200%", null, NodeStatus.Active);
        var result = await service.UpdateAsync(created.Id, updateRequest);

        result.Title.Should().Be("New Title");
        result.Description.Should().Be("New Desc");
        result.Status.Should().Be(NodeStatus.Active);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistingId_ThrowsNotFoundException()
    {
        var service = CreateService(out _);
        var request = new UpdateProcessRequest("Title", null, null, null, null, NodeStatus.NotStarted);

        var act = () => service.UpdateAsync(Guid.NewGuid(), request);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ArchiveAsync_SetsIsArchivedTrue()
    {
        var service = CreateService(out var db);
        var ownerId = Guid.NewGuid();
        var created = await service.CreateAsync(new CreateProcessRequest("To Archive", null, null, null, null), ownerId);

        await service.ArchiveAsync(created.Id);

        var inDb = await db.BusinessProcesses.FindAsync(created.Id);
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
    public async Task LinkInitiativeAsync_CreatesProcessInitiativeLink()
    {
        var service = CreateService(out var db);
        var processId = Guid.NewGuid();
        var initiativeId = Guid.NewGuid();

        await service.LinkInitiativeAsync(processId, initiativeId);

        var link = await db.ProcessInitiatives.FindAsync(processId, initiativeId);
        link.Should().NotBeNull();
        link!.ProcessId.Should().Be(processId);
        link.InitiativeId.Should().Be(initiativeId);
    }

    [Fact]
    public async Task LinkInitiativeAsync_WhenAlreadyLinked_DoesNotCreateDuplicate()
    {
        var service = CreateService(out var db);
        var processId = Guid.NewGuid();
        var initiativeId = Guid.NewGuid();

        await service.LinkInitiativeAsync(processId, initiativeId);
        await service.LinkInitiativeAsync(processId, initiativeId);

        var links = db.ProcessInitiatives
            .Where(pi => pi.ProcessId == processId && pi.InitiativeId == initiativeId).ToList();
        links.Should().HaveCount(1);
    }

    [Fact]
    public async Task UnlinkInitiativeAsync_WithExistingLink_RemovesLink()
    {
        var service = CreateService(out var db);
        var processId = Guid.NewGuid();
        var initiativeId = Guid.NewGuid();

        await service.LinkInitiativeAsync(processId, initiativeId);
        await service.UnlinkInitiativeAsync(processId, initiativeId);

        var link = db.ProcessInitiatives.FirstOrDefault(pi => pi.ProcessId == processId && pi.InitiativeId == initiativeId);
        link.Should().BeNull();
    }

    [Fact]
    public async Task UnlinkInitiativeAsync_WithNonExistingLink_DoesNotThrow()
    {
        var service = CreateService(out _);

        var act = () => service.UnlinkInitiativeAsync(Guid.NewGuid(), Guid.NewGuid());

        await act.Should().NotThrowAsync();
    }
}
