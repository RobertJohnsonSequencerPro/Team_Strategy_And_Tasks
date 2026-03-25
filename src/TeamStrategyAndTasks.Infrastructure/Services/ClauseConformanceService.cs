using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TeamStrategyAndTasks.Core.DTOs;
using TeamStrategyAndTasks.Core.Entities;
using TeamStrategyAndTasks.Core.Enums;
using TeamStrategyAndTasks.Core.Exceptions;
using TeamStrategyAndTasks.Core.Interfaces;
using TeamStrategyAndTasks.Infrastructure.Data;
using TeamStrategyAndTasks.Infrastructure.Identity;

namespace TeamStrategyAndTasks.Infrastructure.Services;

public class ClauseConformanceService(
    IDbContextFactory<AppDbContext> dbFactory,
    UserManager<ApplicationUser> users) : IClauseConformanceService
{
    // ── Queries ──────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<QualityClauseDto>> GetChecklistAsync(CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var clauses = await db.QualityClauses
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .Select(c => new
            {
                c.Id, c.ClauseNumber, c.Title, c.Description, c.SortOrder, c.IsActive,
                c.ConformanceStatus, c.AssignedToId, c.ReviewDueDate,
                c.AssessmentNotes, c.LastReviewedAt,
                EvidenceCount = c.EvidenceItems.Count
            })
            .ToListAsync(ct);

        if (clauses.Count == 0) return [];

        var assigneeIds = clauses
            .Where(c => c.AssignedToId.HasValue)
            .Select(c => c.AssignedToId!.Value)
            .Distinct()
            .ToList();

        var assigneeNames = assigneeIds.Count > 0
            ? await db.Users.AsNoTracking()
                .Where(u => assigneeIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.DisplayName ?? u.UserName ?? u.Id.ToString(), ct)
            : [];

        var now = DateTimeOffset.UtcNow;

        return clauses.Select(c => new QualityClauseDto(
            c.Id, c.ClauseNumber, c.Title, c.Description, c.SortOrder, c.IsActive,
            c.ConformanceStatus,
            c.AssignedToId,
            c.AssignedToId.HasValue ? assigneeNames.GetValueOrDefault(c.AssignedToId.Value) : null,
            c.ReviewDueDate,
            c.AssessmentNotes,
            c.LastReviewedAt,
            c.EvidenceCount,
            IsOverdue: c.ReviewDueDate.HasValue && c.ReviewDueDate.Value < now
                       && c.ConformanceStatus != ConformanceStatus.Conforming))
            .ToList();
    }

    public async Task<QualityClauseDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var c = await db.QualityClauses
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id, x.ClauseNumber, x.Title, x.Description, x.SortOrder, x.IsActive,
                x.ConformanceStatus, x.AssignedToId, x.ReviewDueDate,
                x.AssessmentNotes, x.LastReviewedAt,
                EvidenceCount = x.EvidenceItems.Count
            })
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(QualityClause), id);

        string? assigneeName = null;
        if (c.AssignedToId.HasValue)
        {
            var u = await users.FindByIdAsync(c.AssignedToId.Value.ToString());
            assigneeName = u?.DisplayName ?? u?.UserName;
        }

        var now = DateTimeOffset.UtcNow;
        return new QualityClauseDto(
            c.Id, c.ClauseNumber, c.Title, c.Description, c.SortOrder, c.IsActive,
            c.ConformanceStatus, c.AssignedToId, assigneeName,
            c.ReviewDueDate, c.AssessmentNotes, c.LastReviewedAt, c.EvidenceCount,
            IsOverdue: c.ReviewDueDate.HasValue && c.ReviewDueDate.Value < now
                       && c.ConformanceStatus != ConformanceStatus.Conforming);
    }

    // ── Assessment update ────────────────────────────────────────────────────

    public async Task UpdateAssessmentAsync(
        Guid clauseId, UpdateClauseAssessmentRequest request,
        Guid performedByUserId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var clause = await db.QualityClauses.FindAsync([clauseId], ct)
            ?? throw new NotFoundException(nameof(QualityClause), clauseId);

        var previousStatus = clause.ConformanceStatus;

        clause.ConformanceStatus = request.ConformanceStatus;
        clause.AssignedToId     = request.AssignedToId;
        clause.ReviewDueDate    = request.ReviewDueDate;
        clause.AssessmentNotes  = request.Notes?.Trim();
        clause.LastReviewedAt   = DateTimeOffset.UtcNow;
        clause.LastReviewedById = performedByUserId;

        // Write history record only when status changes
        if (previousStatus != request.ConformanceStatus)
        {
            db.ClauseReviewEvents.Add(new ClauseReviewEvent
            {
                ClauseId       = clauseId,
                PreviousStatus = previousStatus,
                NewStatus      = request.ConformanceStatus,
                ReviewedById   = performedByUserId,
                ReviewedAt     = DateTimeOffset.UtcNow,
                Notes          = request.Notes?.Trim()
            });
        }

        await db.SaveChangesAsync(ct);
    }

    // ── Evidence ─────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<ClauseEvidenceItemDto>> GetEvidenceAsync(
        Guid clauseId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var items = await db.ClauseEvidenceItems
            .AsNoTracking()
            .Where(e => e.ClauseId == clauseId)
            .OrderBy(e => e.CreatedAt)
            .ToListAsync(ct);

        if (items.Count == 0) return [];

        var adderIds = items.Select(e => e.AddedById).Distinct().ToList();
        var names = await db.Users.AsNoTracking()
            .Where(u => adderIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.DisplayName ?? u.UserName ?? u.Id.ToString(), ct);

        return items.Select(e => new ClauseEvidenceItemDto(
            e.Id, e.ClauseId, e.EvidenceType, e.Title, e.Description, e.Url,
            e.AddedById, names.GetValueOrDefault(e.AddedById, e.AddedById.ToString()),
            e.CreatedAt))
            .ToList();
    }

    public async Task<ClauseEvidenceItemDto> AddEvidenceAsync(
        AddEvidenceItemRequest request, Guid performedByUserId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new AppValidationException("Title", "Title is required.");

        if (request.EvidenceType == EvidenceType.Link && string.IsNullOrWhiteSpace(request.Url))
            throw new AppValidationException("Url", "A URL is required for Link evidence.");

        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var item = new ClauseEvidenceItem
        {
            ClauseId     = request.ClauseId,
            EvidenceType = request.EvidenceType,
            Title        = request.Title.Trim(),
            Description  = request.Description?.Trim(),
            Url          = request.Url?.Trim(),
            AddedById    = performedByUserId
        };

        db.ClauseEvidenceItems.Add(item);
        await db.SaveChangesAsync(ct);

        var adder = await users.FindByIdAsync(performedByUserId.ToString());
        var adderName = adder?.DisplayName ?? adder?.UserName ?? performedByUserId.ToString();

        return new ClauseEvidenceItemDto(
            item.Id, item.ClauseId, item.EvidenceType,
            item.Title, item.Description, item.Url,
            item.AddedById, adderName, item.CreatedAt);
    }

    public async Task DeleteEvidenceAsync(
        Guid evidenceId, Guid performedByUserId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var item = await db.ClauseEvidenceItems.FindAsync([evidenceId], ct)
            ?? throw new NotFoundException(nameof(ClauseEvidenceItem), evidenceId);

        db.ClauseEvidenceItems.Remove(item);
        await db.SaveChangesAsync(ct);
    }

    // ── Review history ───────────────────────────────────────────────────────

    public async Task<IReadOnlyList<ClauseReviewEventDto>> GetReviewHistoryAsync(
        Guid clauseId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var events = await db.ClauseReviewEvents
            .AsNoTracking()
            .Where(r => r.ClauseId == clauseId)
            .OrderByDescending(r => r.ReviewedAt)
            .ToListAsync(ct);

        if (events.Count == 0) return [];

        var reviewerIds = events.Select(e => e.ReviewedById).Distinct().ToList();
        var names = await db.Users.AsNoTracking()
            .Where(u => reviewerIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.DisplayName ?? u.UserName ?? u.Id.ToString(), ct);

        return events.Select(e => new ClauseReviewEventDto(
            e.Id, e.ClauseId,
            e.PreviousStatus, e.NewStatus,
            e.ReviewedById, names.GetValueOrDefault(e.ReviewedById, e.ReviewedById.ToString()),
            e.ReviewedAt, e.Notes))
            .ToList();
    }
}
