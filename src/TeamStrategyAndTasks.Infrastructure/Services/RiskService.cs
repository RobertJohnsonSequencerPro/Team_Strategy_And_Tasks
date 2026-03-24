using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TeamStrategyAndTasks.Core.DTOs;
using TeamStrategyAndTasks.Core.Enums;
using TeamStrategyAndTasks.Core.Entities;
using TeamStrategyAndTasks.Core.Interfaces;
using TeamStrategyAndTasks.Infrastructure.Data;
using TeamStrategyAndTasks.Infrastructure.Identity;

namespace TeamStrategyAndTasks.Infrastructure.Services;

public class RiskService(
    IDbContextFactory<AppDbContext> dbFactory,
    IAuditService audit,
    UserManager<ApplicationUser> users) : IRiskService
{
    private const int HighSeverityThreshold = 9; // High × High

    // ── Queries ──────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<NodeRiskDto>> GetForNodeAsync(
        NodeType nodeType, Guid nodeId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var risks = await db.NodeRisks
            .AsNoTracking()
            .Where(r => r.NodeType == nodeType && r.NodeId == nodeId)
            .ToListAsync(ct);

        risks = risks
            .OrderByDescending(r => r.Severity)
            .ThenBy(r => r.Title)
            .ToList();

        if (risks.Count == 0) return [];

        var ownerIds = risks.Select(r => r.OwnerId).Distinct().ToList();
        var ownerNames = await db.Users
            .AsNoTracking()
            .Where(u => ownerIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.DisplayName ?? u.UserName ?? u.Id.ToString(), ct);

        var titles = await ResolveNodeTitlesAsync(db, risks, ct);

        return risks.Select(r => new NodeRiskDto(
                r.Id,
                r.NodeType,
                r.NodeId,
                titles.GetValueOrDefault((r.NodeType, r.NodeId), r.NodeId.ToString()),
                r.Title,
                r.Description,
                r.Probability,
                r.Impact,
                r.Severity,
                r.MitigationPlan,
                r.OwnerId,
                ownerNames.GetValueOrDefault(r.OwnerId, r.OwnerId.ToString()),
                r.Status,
                r.RaisedAt,
                r.ResolvedAt))
            .ToList();
    }

    public async Task<IReadOnlyList<NodeRiskDto>> GetAllOpenAsync(CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var risks = await db.NodeRisks
            .AsNoTracking()
            .Where(r => r.Status == RiskStatus.Open)
            .ToListAsync(ct);

        risks = risks
            .OrderByDescending(r => r.Severity)
            .ThenBy(r => r.NodeType)
            .ThenBy(r => r.Title)
            .ToList();

        if (risks.Count == 0) return [];

        var ownerIds = risks.Select(r => r.OwnerId).Distinct().ToList();
        var ownerNames = await db.Users
            .AsNoTracking()
            .Where(u => ownerIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.DisplayName ?? u.UserName ?? u.Id.ToString(), ct);

        var titles = await ResolveNodeTitlesAsync(db, risks, ct);

        return risks.Select(r => new NodeRiskDto(
                r.Id,
                r.NodeType,
                r.NodeId,
                titles.GetValueOrDefault((r.NodeType, r.NodeId), r.NodeId.ToString()),
                r.Title,
                r.Description,
                r.Probability,
                r.Impact,
                r.Severity,
                r.MitigationPlan,
                r.OwnerId,
                ownerNames.GetValueOrDefault(r.OwnerId, r.OwnerId.ToString()),
                r.Status,
                r.RaisedAt,
                r.ResolvedAt))
            .ToList();
    }

    // ── Mutations ────────────────────────────────────────────────────────────

    public async Task<NodeRiskDto> AddAsync(
        AddRiskRequest request, Guid performedByUserId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        if (string.IsNullOrWhiteSpace(request.Title))
            throw new AppValidationException("Title", "Title is required.");

        var risk = new NodeRisk
        {
            NodeType       = request.NodeType,
            NodeId         = request.NodeId,
            Title          = request.Title.Trim(),
            Description    = request.Description?.Trim(),
            Probability    = request.Probability,
            Impact         = request.Impact,
            MitigationPlan = request.MitigationPlan?.Trim(),
            OwnerId        = request.OwnerId,
            RaisedAt       = DateTimeOffset.UtcNow
        };

        db.NodeRisks.Add(risk);
        await db.SaveChangesAsync(ct);

        await audit.LogAsync(request.NodeType, request.NodeId, performedByUserId,
            "Risk.Added", null, risk.Title, ct);

        // Auto-elevate to At Risk if severity is High×High
        if (risk.Severity >= HighSeverityThreshold)
            await TrySetAtRiskAsync(risk.NodeType, risk.NodeId, ct);

        var owner = await users.FindByIdAsync(risk.OwnerId.ToString());
        var ownerName = owner?.DisplayName ?? owner?.UserName ?? risk.OwnerId.ToString();
        var nodeTitle = await ResolveNodeTitleAsync(db, risk.NodeType, risk.NodeId, ct);

        return new NodeRiskDto(
            risk.Id, risk.NodeType, risk.NodeId, nodeTitle,
            risk.Title, risk.Description,
            risk.Probability, risk.Impact, risk.Severity,
            risk.MitigationPlan,
            risk.OwnerId, ownerName,
            risk.Status, risk.RaisedAt, risk.ResolvedAt);
    }

    public async Task<NodeRiskDto> UpdateAsync(
        Guid id, UpdateRiskRequest request, Guid performedByUserId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        if (string.IsNullOrWhiteSpace(request.Title))
            throw new AppValidationException("Title", "Title is required.");

        var risk = await db.NodeRisks.FindAsync([id], ct)
            ?? throw new NotFoundException($"Risk {id} not found.");

        var oldStatus = risk.Status;
        var oldTitle  = risk.Title;

        risk.Title          = request.Title.Trim();
        risk.Description    = request.Description?.Trim();
        risk.Probability    = request.Probability;
        risk.Impact         = request.Impact;
        risk.MitigationPlan = request.MitigationPlan?.Trim();
        risk.OwnerId        = request.OwnerId;
        risk.Status         = request.Status;

        if (request.Status != RiskStatus.Open && oldStatus == RiskStatus.Open)
            risk.ResolvedAt = DateTimeOffset.UtcNow;
        else if (request.Status == RiskStatus.Open)
            risk.ResolvedAt = null;

        await db.SaveChangesAsync(ct);

        await audit.LogAsync(risk.NodeType, risk.NodeId, performedByUserId,
            "Risk.Updated", oldTitle,
            $"{risk.Title} [{risk.Status}] P={risk.Probability} I={risk.Impact}", ct);

        // Re-evaluate auto At Risk status
        if (risk.Status == RiskStatus.Open && risk.Severity >= HighSeverityThreshold)
            await TrySetAtRiskAsync(risk.NodeType, risk.NodeId, ct);

        var owner = await users.FindByIdAsync(risk.OwnerId.ToString());
        var ownerName = owner?.DisplayName ?? owner?.UserName ?? risk.OwnerId.ToString();
        var nodeTitle = await ResolveNodeTitleAsync(db, risk.NodeType, risk.NodeId, ct);

        return new NodeRiskDto(
            risk.Id, risk.NodeType, risk.NodeId, nodeTitle,
            risk.Title, risk.Description,
            risk.Probability, risk.Impact, risk.Severity,
            risk.MitigationPlan,
            risk.OwnerId, ownerName,
            risk.Status, risk.RaisedAt, risk.ResolvedAt);
    }

    public async Task DeleteAsync(Guid id, Guid performedByUserId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var risk = await db.NodeRisks.FindAsync([id], ct)
            ?? throw new NotFoundException($"Risk {id} not found.");

        var nodeType = risk.NodeType;
        var nodeId   = risk.NodeId;
        var title    = risk.Title;

        db.NodeRisks.Remove(risk);
        await db.SaveChangesAsync(ct);

        await audit.LogAsync(nodeType, nodeId, performedByUserId,
            "Risk.Removed", title, null, ct);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static async Task<string> ResolveNodeTitleAsync(AppDbContext db, NodeType type, Guid id, CancellationToken ct) =>
        type switch
        {
            NodeType.Objective  => (await db.Objectives.Where(o => o.Id == id).Select(o => o.Title).FirstOrDefaultAsync(ct)) ?? id.ToString(),
            NodeType.Process    => (await db.BusinessProcesses.Where(p => p.Id == id).Select(p => p.Title).FirstOrDefaultAsync(ct)) ?? id.ToString(),
            NodeType.Initiative => (await db.Initiatives.Where(i => i.Id == id).Select(i => i.Title).FirstOrDefaultAsync(ct)) ?? id.ToString(),
            NodeType.Task       => (await db.WorkTasks.Where(t => t.Id == id).Select(t => t.Title).FirstOrDefaultAsync(ct)) ?? id.ToString(),
            _                   => id.ToString()
        };

    private static async Task<Dictionary<(NodeType Type, Guid Id), string>> ResolveNodeTitlesAsync(
        AppDbContext db,
        IReadOnlyList<NodeRisk> risks,
        CancellationToken ct)
    {
        var result = new Dictionary<(NodeType Type, Guid Id), string>();

        var objectiveIds = risks.Where(r => r.NodeType == NodeType.Objective).Select(r => r.NodeId).Distinct().ToList();
        var processIds = risks.Where(r => r.NodeType == NodeType.Process).Select(r => r.NodeId).Distinct().ToList();
        var initiativeIds = risks.Where(r => r.NodeType == NodeType.Initiative).Select(r => r.NodeId).Distinct().ToList();
        var taskIds = risks.Where(r => r.NodeType == NodeType.Task).Select(r => r.NodeId).Distinct().ToList();

        if (objectiveIds.Count > 0)
        {
            var rows = await db.Objectives.AsNoTracking().Where(o => objectiveIds.Contains(o.Id)).Select(o => new { o.Id, o.Title }).ToListAsync(ct);
            foreach (var row in rows) result[(NodeType.Objective, row.Id)] = row.Title;
        }

        if (processIds.Count > 0)
        {
            var rows = await db.BusinessProcesses.AsNoTracking().Where(p => processIds.Contains(p.Id)).Select(p => new { p.Id, p.Title }).ToListAsync(ct);
            foreach (var row in rows) result[(NodeType.Process, row.Id)] = row.Title;
        }

        if (initiativeIds.Count > 0)
        {
            var rows = await db.Initiatives.AsNoTracking().Where(i => initiativeIds.Contains(i.Id)).Select(i => new { i.Id, i.Title }).ToListAsync(ct);
            foreach (var row in rows) result[(NodeType.Initiative, row.Id)] = row.Title;
        }

        if (taskIds.Count > 0)
        {
            var rows = await db.WorkTasks.AsNoTracking().Where(t => taskIds.Contains(t.Id)).Select(t => new { t.Id, t.Title }).ToListAsync(ct);
            foreach (var row in rows) result[(NodeType.Task, row.Id)] = row.Title;
        }

        return result;
    }

    private static readonly NodeStatus[] AutoStatuses =
    [
        NodeStatus.NotStarted, NodeStatus.Active, NodeStatus.InProgress, NodeStatus.OnTrack
    ];

    private async Task TrySetAtRiskAsync(NodeType type, Guid id, CancellationToken ct)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        switch (type)
        {
            case NodeType.Objective:
                var obj = await db.Objectives.FindAsync([id], ct);
                if (obj is not null && AutoStatuses.Contains(obj.Status))
                { obj.Status = NodeStatus.AtRisk; await db.SaveChangesAsync(ct); }
                break;
            case NodeType.Process:
                var proc = await db.BusinessProcesses.FindAsync([id], ct);
                if (proc is not null && AutoStatuses.Contains(proc.Status))
                { proc.Status = NodeStatus.AtRisk; await db.SaveChangesAsync(ct); }
                break;
            case NodeType.Initiative:
                var init = await db.Initiatives.FindAsync([id], ct);
                if (init is not null && AutoStatuses.Contains(init.Status))
                { init.Status = NodeStatus.AtRisk; await db.SaveChangesAsync(ct); }
                break;
            case NodeType.Task:
                var task = await db.WorkTasks.FindAsync([id], ct);
                if (task is not null && AutoStatuses.Contains(task.Status))
                { task.Status = NodeStatus.AtRisk; await db.SaveChangesAsync(ct); }
                break;
        }
    }
}
