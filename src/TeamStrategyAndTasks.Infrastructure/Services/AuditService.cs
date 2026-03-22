using Microsoft.EntityFrameworkCore;
using TeamStrategyAndTasks.Core.Entities;
using TeamStrategyAndTasks.Core.Enums;
using TeamStrategyAndTasks.Core.Interfaces;
using TeamStrategyAndTasks.Infrastructure.Data;

namespace TeamStrategyAndTasks.Infrastructure.Services;

public class AuditService(AppDbContext db) : IAuditService
{
    public async Task LogAsync(
        NodeType nodeType, Guid nodeId, Guid userId,
        string fieldName, string? oldValue, string? newValue,
        CancellationToken ct = default)
    {
        db.AuditLogs.Add(new AuditLog
        {
            NodeType = nodeType,
            NodeId = nodeId,
            UserId = userId,
            FieldName = fieldName,
            OldValue = oldValue,
            NewValue = newValue
        });
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<AuditLog>> GetLogsForNodeAsync(
        NodeType nodeType, Guid nodeId, CancellationToken ct = default) =>
        await db.AuditLogs
            .Where(a => a.NodeType == nodeType && a.NodeId == nodeId)
            .OrderByDescending(a => a.OccurredAt)
            .ToListAsync(ct);
}
