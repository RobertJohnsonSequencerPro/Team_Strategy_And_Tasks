using FluentAssertions;
using TeamStrategyAndTasks.Core.Entities;
using TeamStrategyAndTasks.Core.Enums;
using TeamStrategyAndTasks.Infrastructure.Services;
using TeamStrategyAndTasks.Integration.Tests.Helpers;

namespace TeamStrategyAndTasks.Integration.Tests.Services;

public class AuditServiceTests
{
    private static AuditService CreateService(out Infrastructure.Data.AppDbContext db)
    {
        db = InMemoryDbHelper.CreateContext();
        return new AuditService(db);
    }

    [Fact]
    public async Task LogAsync_CreatesAuditLogEntry()
    {
        var service = CreateService(out var db);
        var nodeId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await service.LogAsync(NodeType.Objective, nodeId, userId, "Title", "Old Title", "New Title");

        var logs = db.AuditLogs.ToList();
        logs.Should().ContainSingle();
        var log = logs[0];
        log.NodeType.Should().Be(NodeType.Objective);
        log.NodeId.Should().Be(nodeId);
        log.UserId.Should().Be(userId);
        log.FieldName.Should().Be("Title");
        log.OldValue.Should().Be("Old Title");
        log.NewValue.Should().Be("New Title");
    }

    [Fact]
    public async Task LogAsync_WithNullOldAndNewValues_CreateEntryWithNulls()
    {
        var service = CreateService(out var db);
        var nodeId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await service.LogAsync(NodeType.Task, nodeId, userId, "Description", null, null);

        var logs = db.AuditLogs.ToList();
        logs.Should().ContainSingle();
        logs[0].OldValue.Should().BeNull();
        logs[0].NewValue.Should().BeNull();
    }

    [Fact]
    public async Task LogAsync_MultipleEntries_AllArePersisted()
    {
        var service = CreateService(out var db);
        var nodeId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await service.LogAsync(NodeType.Objective, nodeId, userId, "Title", "Old", "New");
        await service.LogAsync(NodeType.Objective, nodeId, userId, "Status", "NotStarted", "InProgress");

        var logs = db.AuditLogs.ToList();
        logs.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetLogsForNodeAsync_ReturnsLogsForSpecifiedNode()
    {
        var service = CreateService(out _);
        var nodeId = Guid.NewGuid();
        var otherNodeId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await service.LogAsync(NodeType.Objective, nodeId, userId, "Title", "Old", "New");
        await service.LogAsync(NodeType.Objective, otherNodeId, userId, "Title", "Old2", "New2");

        var logs = await service.GetLogsForNodeAsync(NodeType.Objective, nodeId);

        logs.Should().ContainSingle();
        logs[0].NodeId.Should().Be(nodeId);
    }

    [Fact]
    public async Task GetLogsForNodeAsync_FiltersByNodeType()
    {
        var service = CreateService(out _);
        var nodeId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await service.LogAsync(NodeType.Objective, nodeId, userId, "Title", "Old", "New");
        await service.LogAsync(NodeType.Task, nodeId, userId, "Status", "NotStarted", "Done");

        var objectiveLogs = await service.GetLogsForNodeAsync(NodeType.Objective, nodeId);
        var taskLogs = await service.GetLogsForNodeAsync(NodeType.Task, nodeId);

        objectiveLogs.Should().ContainSingle();
        taskLogs.Should().ContainSingle();
        objectiveLogs[0].FieldName.Should().Be("Title");
        taskLogs[0].FieldName.Should().Be("Status");
    }

    [Fact]
    public async Task GetLogsForNodeAsync_ReturnsLogsOrderedByOccurredAtDescending()
    {
        var service = CreateService(out _);
        var nodeId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await service.LogAsync(NodeType.Objective, nodeId, userId, "Field1", "Old1", "New1");
        await Task.Delay(10); // Ensure different timestamps
        await service.LogAsync(NodeType.Objective, nodeId, userId, "Field2", "Old2", "New2");

        var logs = await service.GetLogsForNodeAsync(NodeType.Objective, nodeId);

        logs.Should().HaveCount(2);
        logs[0].OccurredAt.Should().BeOnOrAfter(logs[1].OccurredAt);
    }

    [Fact]
    public async Task GetLogsForNodeAsync_WithNoMatchingLogs_ReturnsEmptyList()
    {
        var service = CreateService(out _);

        var logs = await service.GetLogsForNodeAsync(NodeType.Objective, Guid.NewGuid());

        logs.Should().BeEmpty();
    }

    [Theory]
    [InlineData(NodeType.Objective)]
    [InlineData(NodeType.Process)]
    [InlineData(NodeType.Initiative)]
    [InlineData(NodeType.Task)]
    public async Task LogAsync_WithAllNodeTypes_CreatesEntrySuccessfully(NodeType nodeType)
    {
        var service = CreateService(out var db);
        var nodeId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await service.LogAsync(nodeType, nodeId, userId, "Field", "Old", "New");

        var logs = db.AuditLogs.ToList();
        logs.Should().ContainSingle();
        logs[0].NodeType.Should().Be(nodeType);
    }
}
