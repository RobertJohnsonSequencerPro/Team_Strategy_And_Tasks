using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TeamStrategyAndTasks.Core.DTOs;
using TeamStrategyAndTasks.Core.Entities;
using TeamStrategyAndTasks.Core.Enums;
using TeamStrategyAndTasks.Core.Interfaces;
using TeamStrategyAndTasks.Infrastructure.Data;
using TeamStrategyAndTasks.Infrastructure.Identity;

namespace TeamStrategyAndTasks.Infrastructure.Services;

public class DecisionService(
    IDbContextFactory<AppDbContext> dbFactory,
    IAuditService audit) : IDecisionService
{
    // ── Queries ──────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<DecisionDto>> GetAllAsync(CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var decisions = await db.Decisions
            .AsNoTracking()
            .Include(d => d.NodeLinks)
            .OrderByDescending(d => d.MadeAt)
            .ToListAsync(ct);

        return await MapManyAsync(db, decisions, ct);
    }

    public async Task<IReadOnlyList<DecisionDto>> GetForNodeAsync(
        NodeType nodeType, Guid nodeId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var decisionIds = await db.DecisionNodeLinks
            .AsNoTracking()
            .Where(nl => nl.NodeType == nodeType && nl.NodeId == nodeId)
            .Select(nl => nl.DecisionId)
            .ToListAsync(ct);

        if (decisionIds.Count == 0) return [];

        var decisions = await db.Decisions
            .AsNoTracking()
            .Include(d => d.NodeLinks)
            .Where(d => decisionIds.Contains(d.Id))
            .OrderByDescending(d => d.MadeAt)
            .ToListAsync(ct);

        return await MapManyAsync(db, decisions, ct);
    }

    public async Task<DecisionDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var decision = await db.Decisions
            .AsNoTracking()
            .Include(d => d.NodeLinks)
            .FirstOrDefaultAsync(d => d.Id == id, ct);

        if (decision is null) return null;
        return (await MapManyAsync(db, [decision], ct)).FirstOrDefault();
    }

    // ── Mutations ─────────────────────────────────────────────────────────────

    public async Task<DecisionDto> AddAsync(
        AddDecisionRequest request, Guid madeByUserId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var decision = new Decision
        {
            Title                  = request.Title,
            Context                = request.Context,
            Rationale              = request.Rationale,
            AlternativesConsidered = request.AlternativesConsidered,
            MadeById               = madeByUserId,
            MadeAt                 = request.MadeAt,
            Status                 = DecisionStatus.Open
        };

        if (request.InitialNodeType.HasValue && request.InitialNodeId.HasValue)
        {
            decision.NodeLinks.Add(new DecisionNodeLink
            {
                DecisionId = decision.Id,
                NodeType   = request.InitialNodeType.Value,
                NodeId     = request.InitialNodeId.Value
            });
        }

        db.Decisions.Add(decision);
        await db.SaveChangesAsync(ct);

        await audit.LogAsync(
            NodeType.Objective, // placeholder — decisions are global, not node-specific
            decision.Id,
            madeByUserId,
            "Decision",
            null,
            decision.Title,
            ct);

        return (await GetByIdAsync(decision.Id, ct))!;
    }

    public async Task<DecisionDto> UpdateAsync(
        Guid id, UpdateDecisionRequest request, Guid performedByUserId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var decision = await db.Decisions.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Decision {id} not found.");

        var oldTitle = decision.Title;

        decision.Title                  = request.Title;
        decision.Context                = request.Context;
        decision.Rationale              = request.Rationale;
        decision.AlternativesConsidered = request.AlternativesConsidered;
        decision.MadeAt                 = request.MadeAt;

        await db.SaveChangesAsync(ct);

        if (oldTitle != request.Title)
            await audit.LogAsync(NodeType.Objective, id, performedByUserId, "Title", oldTitle, request.Title, ct);

        return (await GetByIdAsync(id, ct))!;
    }

    public async Task SupersedeAsync(
        Guid oldDecisionId, Guid newDecisionId, Guid performedByUserId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var old = await db.Decisions.FindAsync([oldDecisionId], ct)
            ?? throw new KeyNotFoundException($"Decision {oldDecisionId} not found.");

        old.Status        = DecisionStatus.Superseded;
        old.SupersededById = newDecisionId;

        await db.SaveChangesAsync(ct);

        await audit.LogAsync(NodeType.Objective, oldDecisionId, performedByUserId,
            "Status", nameof(DecisionStatus.Open), nameof(DecisionStatus.Superseded), ct);
    }

    public async Task LinkNodeAsync(
        Guid decisionId, NodeType nodeType, Guid nodeId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var exists = await db.DecisionNodeLinks.AnyAsync(
            nl => nl.DecisionId == decisionId && nl.NodeType == nodeType && nl.NodeId == nodeId, ct);

        if (exists) return;

        db.DecisionNodeLinks.Add(new DecisionNodeLink
        {
            DecisionId = decisionId,
            NodeType   = nodeType,
            NodeId     = nodeId
        });

        await db.SaveChangesAsync(ct);
    }

    public async Task UnlinkNodeAsync(
        Guid decisionId, NodeType nodeType, Guid nodeId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var link = await db.DecisionNodeLinks.FirstOrDefaultAsync(
            nl => nl.DecisionId == decisionId && nl.NodeType == nodeType && nl.NodeId == nodeId, ct);

        if (link is null) return;

        db.DecisionNodeLinks.Remove(link);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, Guid performedByUserId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var decision = await db.Decisions.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Decision {id} not found.");

        db.Decisions.Remove(decision);
        await db.SaveChangesAsync(ct);
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    private async Task<IReadOnlyList<DecisionDto>> MapManyAsync(
        AppDbContext db, IList<Decision> decisions, CancellationToken ct)
    {
        if (decisions.Count == 0) return [];

        // Resolve user display names
        var userIds = decisions.Select(d => d.MadeById).Distinct().ToList();
        var userNames = await db.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.DisplayName ?? u.UserName ?? u.Id.ToString(), ct);

        // Resolve superseding decision titles
        var supersededByIds = decisions
            .Where(d => d.SupersededById.HasValue)
            .Select(d => d.SupersededById!.Value)
            .Distinct()
            .ToList();

        Dictionary<Guid, string> supersededByTitles = [];
        if (supersededByIds.Count > 0)
        {
            supersededByTitles = await db.Decisions
                .AsNoTracking()
                .Where(d => supersededByIds.Contains(d.Id))
                .ToDictionaryAsync(d => d.Id, d => d.Title, ct);
        }

        // Resolve node titles for all NodeLinks
        var allLinks = decisions.SelectMany(d => d.NodeLinks).ToList();
        var nodeTitles = await ResolveNodeTitlesAsync(db, allLinks, ct);

        return decisions.Select(d => new DecisionDto(
            d.Id,
            d.Title,
            d.Context,
            d.Rationale,
            d.AlternativesConsidered,
            d.MadeById,
            userNames.GetValueOrDefault(d.MadeById, d.MadeById.ToString()),
            d.MadeAt,
            d.Status,
            d.SupersededById,
            d.SupersededById.HasValue
                ? supersededByTitles.GetValueOrDefault(d.SupersededById.Value)
                : null,
            d.NodeLinks.Select(nl => new DecisionNodeLinkDto(
                nl.NodeType,
                nl.NodeId,
                nodeTitles.GetValueOrDefault((nl.NodeType, nl.NodeId), nl.NodeId.ToString())))
            .ToList(),
            d.CreatedAt,
            d.UpdatedAt))
        .ToList();
    }

    private static async Task<Dictionary<(NodeType, Guid), string>> ResolveNodeTitlesAsync(
        AppDbContext db, IList<DecisionNodeLink> links, CancellationToken ct)
    {
        var result = new Dictionary<(NodeType, Guid), string>();
        if (links.Count == 0) return result;

        // Group by node type and batch-load titles
        var groupedByType = links.GroupBy(l => l.NodeType).ToDictionary(g => g.Key, g => g.Select(l => l.NodeId).ToHashSet());

        if (groupedByType.TryGetValue(NodeType.Objective, out var objIds))
            (await db.Objectives.AsNoTracking().Where(x => objIds.Contains(x.Id)).Select(x => new { x.Id, x.Title }).ToListAsync(ct))
                .ForEach(x => result[(NodeType.Objective, x.Id)] = x.Title);

        if (groupedByType.TryGetValue(NodeType.Process, out var procIds))
            (await db.BusinessProcesses.AsNoTracking().Where(x => procIds.Contains(x.Id)).Select(x => new { x.Id, x.Title }).ToListAsync(ct))
                .ForEach(x => result[(NodeType.Process, x.Id)] = x.Title);

        if (groupedByType.TryGetValue(NodeType.Initiative, out var initIds))
            (await db.Initiatives.AsNoTracking().Where(x => initIds.Contains(x.Id)).Select(x => new { x.Id, x.Title }).ToListAsync(ct))
                .ForEach(x => result[(NodeType.Initiative, x.Id)] = x.Title);

        if (groupedByType.TryGetValue(NodeType.Task, out var taskIds))
            (await db.WorkTasks.AsNoTracking().Where(x => taskIds.Contains(x.Id)).Select(x => new { x.Id, x.Title }).ToListAsync(ct))
                .ForEach(x => result[(NodeType.Task, x.Id)] = x.Title);

        return result;
    }
}
