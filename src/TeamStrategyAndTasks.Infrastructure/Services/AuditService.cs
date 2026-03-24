using Microsoft.EntityFrameworkCore;
using TeamStrategyAndTasks.Core.Entities;
using TeamStrategyAndTasks.Core.Enums;
using TeamStrategyAndTasks.Core.Exceptions;
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

    public async Task RevertFieldAsync(
        NodeType nodeType, Guid nodeId, long auditLogId,
        Guid requestingUserId, CancellationToken ct = default)
    {
        var log = await db.AuditLogs
            .FirstOrDefaultAsync(a => a.Id == auditLogId, ct)
            ?? throw new NotFoundException(nameof(AuditLog), auditLogId);

        if (log.NodeType != nodeType || log.NodeId != nodeId)
            throw new ForbiddenException();

        string? currentValue;

        switch (nodeType)
        {
            case NodeType.Objective:
            {
                var entity = await db.Objectives.FindAsync([nodeId], ct)
                    ?? throw new NotFoundException(nameof(Objective), nodeId);
                currentValue = GetObjectiveField(entity, log.FieldName);
                SetObjectiveField(entity, log.FieldName, log.OldValue);
                await db.SaveChangesAsync(ct);
                break;
            }
            case NodeType.Process:
            {
                var entity = await db.BusinessProcesses.FindAsync([nodeId], ct)
                    ?? throw new NotFoundException(nameof(BusinessProcess), nodeId);
                currentValue = GetProcessField(entity, log.FieldName);
                SetProcessField(entity, log.FieldName, log.OldValue);
                await db.SaveChangesAsync(ct);
                break;
            }
            case NodeType.Initiative:
            {
                var entity = await db.Initiatives.FindAsync([nodeId], ct)
                    ?? throw new NotFoundException(nameof(Initiative), nodeId);
                currentValue = GetInitiativeField(entity, log.FieldName);
                SetInitiativeField(entity, log.FieldName, log.OldValue);
                await db.SaveChangesAsync(ct);
                break;
            }
            case NodeType.Task:
            {
                var entity = await db.WorkTasks.FindAsync([nodeId], ct)
                    ?? throw new NotFoundException(nameof(WorkTask), nodeId);
                currentValue = GetTaskField(entity, log.FieldName);
                SetTaskField(entity, log.FieldName, log.OldValue);
                await db.SaveChangesAsync(ct);
                break;
            }
            default:
                throw new InvalidOperationException($"Unsupported node type: {nodeType}");
        }

        await LogAsync(nodeType, nodeId, requestingUserId, log.FieldName, currentValue, log.OldValue, ct);
    }

    // ── Objective field helpers ──────────────────────────────────────────────

    private static string? GetObjectiveField(Objective e, string field) => field switch
    {
        "Title" => e.Title,
        "Description" => e.Description,
        "SuccessMetric" => e.SuccessMetric,
        "TargetValue" => e.TargetValue,
        "TargetDate" => e.TargetDate?.ToString("o"),
        "Status" => e.Status.ToString(),
        _ => null
    };

    private static void SetObjectiveField(Objective e, string field, string? value)
    {
        switch (field)
        {
            case "Title": e.Title = value ?? string.Empty; break;
            case "Description": e.Description = value; break;
            case "SuccessMetric": e.SuccessMetric = value; break;
            case "TargetValue": e.TargetValue = value; break;
            case "TargetDate": e.TargetDate = value is null ? null : DateTimeOffset.Parse(value); break;
            case "Status": e.Status = Enum.Parse<NodeStatus>(value!); break;
        }
    }

    // ── Process field helpers ────────────────────────────────────────────────

    private static string? GetProcessField(BusinessProcess e, string field) => field switch
    {
        "Title" => e.Title,
        "Description" => e.Description,
        "SuccessMetric" => e.SuccessMetric,
        "TargetValue" => e.TargetValue,
        "TargetDate" => e.TargetDate?.ToString("o"),
        "Status" => e.Status.ToString(),
        _ => null
    };

    private static void SetProcessField(BusinessProcess e, string field, string? value)
    {
        switch (field)
        {
            case "Title": e.Title = value ?? string.Empty; break;
            case "Description": e.Description = value; break;
            case "SuccessMetric": e.SuccessMetric = value; break;
            case "TargetValue": e.TargetValue = value; break;
            case "TargetDate": e.TargetDate = value is null ? null : DateTimeOffset.Parse(value); break;
            case "Status": e.Status = Enum.Parse<NodeStatus>(value!); break;
        }
    }

    // ── Initiative field helpers ─────────────────────────────────────────────

    private static string? GetInitiativeField(Initiative e, string field) => field switch
    {
        "Title" => e.Title,
        "Description" => e.Description,
        "TargetDate" => e.TargetDate?.ToString("o"),
        "Status" => e.Status.ToString(),
        _ => null
    };

    private static void SetInitiativeField(Initiative e, string field, string? value)
    {
        switch (field)
        {
            case "Title": e.Title = value ?? string.Empty; break;
            case "Description": e.Description = value; break;
            case "TargetDate": e.TargetDate = value is null ? null : DateTimeOffset.Parse(value); break;
            case "Status": e.Status = Enum.Parse<NodeStatus>(value!); break;
        }
    }

    // ── Task field helpers ───────────────────────────────────────────────────

    private static string? GetTaskField(WorkTask e, string field) => field switch
    {
        "Title" => e.Title,
        "Description" => e.Description,
        "AssigneeId" => e.AssigneeId?.ToString(),
        "EstimatedEffort" => e.EstimatedEffort?.ToString(),
        "ActualEffort" => e.ActualEffort?.ToString(),
        "TargetDate" => e.TargetDate?.ToString("o"),
        "Status" => e.Status.ToString(),
        _ => null
    };

    private static void SetTaskField(WorkTask e, string field, string? value)
    {
        switch (field)
        {
            case "Title": e.Title = value ?? string.Empty; break;
            case "Description": e.Description = value; break;
            case "AssigneeId": e.AssigneeId = value is null ? null : Guid.Parse(value); break;
            case "EstimatedEffort": e.EstimatedEffort = value is null ? null : decimal.Parse(value); break;
            case "ActualEffort": e.ActualEffort = value is null ? null : decimal.Parse(value); break;
            case "TargetDate": e.TargetDate = value is null ? null : DateTimeOffset.Parse(value); break;
            case "Status": e.Status = Enum.Parse<NodeStatus>(value!); break;
        }
    }
}
