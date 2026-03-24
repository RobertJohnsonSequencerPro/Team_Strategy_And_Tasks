using Microsoft.AspNetCore.Identity;
using TeamStrategyAndTasks.Core.DTOs;
using TeamStrategyAndTasks.Core.Enums;
using TeamStrategyAndTasks.Core.Interfaces;
using TeamStrategyAndTasks.Infrastructure.Identity;

namespace TeamStrategyAndTasks.Infrastructure.Services;

public class RiskService(
    AppDbContext db,
    IAuditService audit,
    UserManager<ApplicationUser> users) : IRiskService
{
    private const int HighSeverityThreshold = 9; // High × High

    // ── Queries ──────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<NodeRiskDto>> GetForNodeAsync(
        NodeType nodeType, Guid nodeId, CancellationToken ct = default)
    {
        var risks = await db.NodeRisks
            .Where(r => r.NodeType == nodeType && r.NodeId == nodeId)
            .OrderByDescending(r => (int)r.Probability * (int)r.Impact)
            .ThenBy(r => r.Title)
            .ToListAsync(ct);

        var result = new List<NodeRiskDto>(risks.Count);
        foreach (var r in risks)
            result.Add(await ToDtoAsync(r, nodeTitle: null, ct));
        return result;
    }

    public async Task<IReadOnlyList<NodeRiskDto>> GetAllOpenAsync(CancellationToken ct = default)
    {
        var risks = await db.NodeRisks
            .Where(r => r.Status == RiskStatus.Open)
            .OrderByDescending(r => (int)r.Probability * (int)r.Impact)
            .ThenBy(r => r.NodeType)
            .ToListAsync(ct);

        var result = new List<NodeRiskDto>(risks.Count);
        foreach (var r in risks)
            result.Add(await ToDtoAsync(r, nodeTitle: null, ct));
        return result;
    }

    // ── Mutations ────────────────────────────────────────────────────────────

    public async Task<NodeRiskDto> AddAsync(
        AddRiskRequest request, Guid performedByUserId, CancellationToken ct = default)
    {
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

        return await ToDtoAsync(risk, nodeTitle: null, ct);
    }

    public async Task<NodeRiskDto> UpdateAsync(
        Guid id, UpdateRiskRequest request, Guid performedByUserId, CancellationToken ct = default)
    {
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

        return await ToDtoAsync(risk, nodeTitle: null, ct);
    }

    public async Task DeleteAsync(Guid id, Guid performedByUserId, CancellationToken ct = default)
    {
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

    private async Task<NodeRiskDto> ToDtoAsync(NodeRisk r, string? nodeTitle, CancellationToken ct)
    {
        var owner = await users.FindByIdAsync(r.OwnerId.ToString());
        var ownerName = owner?.DisplayName ?? owner?.UserName ?? r.OwnerId.ToString();

        if (nodeTitle is null)
            nodeTitle = await ResolveNodeTitleAsync(r.NodeType, r.NodeId, ct);

        return new NodeRiskDto(
            r.Id, r.NodeType, r.NodeId, nodeTitle,
            r.Title, r.Description,
            r.Probability, r.Impact, r.Severity,
            r.MitigationPlan,
            r.OwnerId, ownerName,
            r.Status, r.RaisedAt, r.ResolvedAt);
    }

    private async Task<string> ResolveNodeTitleAsync(NodeType type, Guid id, CancellationToken ct) =>
        type switch
        {
            NodeType.Objective  => (await db.Objectives.Where(o => o.Id == id).Select(o => o.Title).FirstOrDefaultAsync(ct)) ?? id.ToString(),
            NodeType.Process    => (await db.BusinessProcesses.Where(p => p.Id == id).Select(p => p.Title).FirstOrDefaultAsync(ct)) ?? id.ToString(),
            NodeType.Initiative => (await db.Initiatives.Where(i => i.Id == id).Select(i => i.Title).FirstOrDefaultAsync(ct)) ?? id.ToString(),
            NodeType.Task       => (await db.WorkTasks.Where(t => t.Id == id).Select(t => t.Title).FirstOrDefaultAsync(ct)) ?? id.ToString(),
            _                   => id.ToString()
        };

    private static readonly NodeStatus[] AutoStatuses =
    [
        NodeStatus.NotStarted, NodeStatus.Active, NodeStatus.InProgress, NodeStatus.OnTrack
    ];

    private async Task TrySetAtRiskAsync(NodeType type, Guid id, CancellationToken ct)
    {
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
