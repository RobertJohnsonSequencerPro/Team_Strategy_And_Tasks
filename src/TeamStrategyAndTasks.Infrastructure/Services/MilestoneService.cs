using Microsoft.AspNetCore.Identity;
using TeamStrategyAndTasks.Core.DTOs;
using TeamStrategyAndTasks.Core.Enums;
using TeamStrategyAndTasks.Core.Interfaces;
using TeamStrategyAndTasks.Infrastructure.Identity;

namespace TeamStrategyAndTasks.Infrastructure.Services;

public class MilestoneService(
    IDbContextFactory<AppDbContext> dbFactory,
    IAuditService audit,
    UserManager<ApplicationUser> users) : IMilestoneService
{
    // ── Queries ──────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<MilestoneDto>> GetForInitiativeAsync(
        Guid initiativeId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var milestones = await db.Milestones
            .Where(m => m.InitiativeId == initiativeId)
            .OrderBy(m => m.DueDate)
            .ToListAsync(ct);

        var result = new List<MilestoneDto>(milestones.Count);
        foreach (var m in milestones)
            result.Add(await ToDtoAsync(m, ct));

        return result;
    }

    // ── Mutations ────────────────────────────────────────────────────────────

    public async Task<MilestoneDto> AddAsync(
        AddMilestoneRequest request, Guid performedByUserId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        if (string.IsNullOrWhiteSpace(request.Title))
            throw new AppValidationException("Title", "Title is required.");

        if (!await db.Initiatives.AnyAsync(i => i.Id == request.InitiativeId, ct))
            throw new NotFoundException($"Initiative {request.InitiativeId} not found.");

        var milestone = new Milestone
        {
            InitiativeId = request.InitiativeId,
            Title        = request.Title.Trim(),
            DueDate      = request.DueDate,
            OwnerId      = request.OwnerId,
            Notes        = request.Notes?.Trim()
        };

        db.Milestones.Add(milestone);
        await db.SaveChangesAsync(ct);

        await audit.LogAsync(NodeType.Initiative, request.InitiativeId, performedByUserId,
            "Milestone.Added", null, milestone.Title, ct);

        return await ToDtoAsync(milestone, ct);
    }

    public async Task<MilestoneDto> UpdateAsync(
        Guid id, UpdateMilestoneRequest request, Guid performedByUserId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        if (string.IsNullOrWhiteSpace(request.Title))
            throw new AppValidationException("Title", "Title is required.");

        var milestone = await db.Milestones.FindAsync([id], ct)
            ?? throw new NotFoundException($"Milestone {id} not found.");

        var oldTitle = milestone.Title;
        milestone.Title   = request.Title.Trim();
        milestone.DueDate = request.DueDate;
        milestone.OwnerId = request.OwnerId;
        milestone.Notes   = request.Notes?.Trim();

        await db.SaveChangesAsync(ct);

        await audit.LogAsync(NodeType.Initiative, milestone.InitiativeId, performedByUserId,
            "Milestone.Updated", oldTitle, milestone.Title, ct);

        return await ToDtoAsync(milestone, ct);
    }

    public async Task<MilestoneDto> MarkReachedAsync(
        Guid id, Guid performedByUserId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var milestone = await db.Milestones.FindAsync([id], ct)
            ?? throw new NotFoundException($"Milestone {id} not found.");

        if (milestone.Status == MilestoneStatus.Reached)
            return await ToDtoAsync(milestone, ct); // idempotent

        var old = milestone.Status.ToString();
        milestone.Status      = MilestoneStatus.Reached;
        milestone.CompletedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);

        await audit.LogAsync(NodeType.Initiative, milestone.InitiativeId, performedByUserId,
            "Milestone.Status", old, nameof(MilestoneStatus.Reached), ct);

        return await ToDtoAsync(milestone, ct);
    }

    public async Task DeleteAsync(Guid id, Guid performedByUserId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var milestone = await db.Milestones.FindAsync([id], ct)
            ?? throw new NotFoundException($"Milestone {id} not found.");

        var initiativeId = milestone.InitiativeId;
        var title        = milestone.Title;

        db.Milestones.Remove(milestone);
        await db.SaveChangesAsync(ct);

        await audit.LogAsync(NodeType.Initiative, initiativeId, performedByUserId,
            "Milestone.Removed", title, null, ct);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task<MilestoneDto> ToDtoAsync(Milestone m, CancellationToken ct)
    {
        var owner = await users.FindByIdAsync(m.OwnerId.ToString());
        var ownerName = owner?.DisplayName ?? owner?.UserName ?? m.OwnerId.ToString();
        var isOverdue = m.Status == MilestoneStatus.Pending && m.DueDate < DateTimeOffset.UtcNow;
        return new MilestoneDto(
            m.Id, m.InitiativeId, m.Title, m.DueDate,
            m.Status, m.CompletedAt, m.Notes,
            m.OwnerId, ownerName, isOverdue);
    }
}
