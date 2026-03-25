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

public class CapaService(
    IDbContextFactory<AppDbContext> dbFactory,
    UserManager<ApplicationUser> users) : ICapaService
{
    private static readonly CapaCaseStatus[] CaseWorkflow =
    [
        CapaCaseStatus.Open,
        CapaCaseStatus.Containment,
        CapaCaseStatus.RootCauseAnalysis,
        CapaCaseStatus.CorrectiveAction,
        CapaCaseStatus.EffectivenessVerification,
        CapaCaseStatus.Closed
    ];

    // ── List ─────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<CapaCaseSummaryDto>> GetAllAsync(
        bool showArchived = false, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var cases = await db.CapaCases
            .AsNoTracking()
            .Where(c => showArchived || !c.IsArchived)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new
            {
                c.Id, c.Title, c.CaseNumber, c.CapaType, c.Status,
                c.OwnerId, c.TargetDate, c.CompletedDate,
                c.LinkedFindingId, c.IsArchived, c.CreatedAt, c.UpdatedAt,
                ActionCount     = c.Actions.Count,
                OpenActionCount = c.Actions.Count(a => a.Status != CapaActionStatus.Closed)
            })
            .ToListAsync(ct);

        if (cases.Count == 0) return [];

        var ownerIds = cases
            .Where(c => c.OwnerId.HasValue)
            .Select(c => c.OwnerId!.Value)
            .Distinct()
            .ToList();

        var ownerNames = ownerIds.Count > 0
            ? await db.Users.AsNoTracking()
                .Where(u => ownerIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.DisplayName ?? u.UserName ?? u.Id.ToString(), ct)
            : new Dictionary<Guid, string>();

        var now = DateTimeOffset.UtcNow;

        return cases.Select(c => new CapaCaseSummaryDto(
            c.Id, c.Title, c.CaseNumber, c.CapaType, c.Status,
            c.OwnerId,
            c.OwnerId.HasValue ? ownerNames.GetValueOrDefault(c.OwnerId.Value) : null,
            c.TargetDate, c.CompletedDate, c.LinkedFindingId, c.IsArchived,
            c.ActionCount, c.OpenActionCount,
            IsOverdue: c.TargetDate.HasValue && c.TargetDate < now && c.Status != CapaCaseStatus.Closed,
            c.CreatedAt, c.UpdatedAt)).ToList();
    }

    // ── Detail ───────────────────────────────────────────────────────────────

    public async Task<CapaCaseDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var cas = await db.CapaCases
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new
            {
                c.Id, c.Title, c.CaseNumber, c.CapaType, c.Status,
                c.ProblemStatement, c.ContainmentActions, c.RootCauseAnalysis, c.ProposedCorrection,
                c.OwnerId, c.TargetDate, c.CompletedDate,
                c.LinkedFindingId, c.IsArchived, c.CreatedAt, c.UpdatedAt,
                Actions = c.Actions
                    .OrderBy(a => a.CreatedAt)
                    .Select(a => new
                    {
                        a.Id, a.CapaCaseId, a.Description, a.AssignedToId,
                        a.TargetDate, a.CompletionDate, a.Status, a.Notes, a.CreatedAt
                    }).ToList(),
                Checks = c.EffectivenessChecks
                    .OrderByDescending(e => e.CheckDate)
                    .Select(e => new
                    {
                        e.Id, e.CapaCaseId, e.CheckDate, e.CheckedById,
                        e.Verdict, e.Notes, e.NextCheckDate
                    }).ToList()
            })
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(CapaCase), id);

        var allUserIds = new HashSet<Guid>();
        if (cas.OwnerId.HasValue) allUserIds.Add(cas.OwnerId.Value);
        foreach (var a in cas.Actions) if (a.AssignedToId.HasValue) allUserIds.Add(a.AssignedToId.Value);
        foreach (var e in cas.Checks) allUserIds.Add(e.CheckedById);

        var userNames = allUserIds.Count > 0
            ? await db.Users.AsNoTracking()
                .Where(u => allUserIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.DisplayName ?? u.UserName ?? u.Id.ToString(), ct)
            : new Dictionary<Guid, string>();

        var now = DateTimeOffset.UtcNow;

        var actionDtos = cas.Actions.Select(a => new CapaActionDto(
            a.Id, a.CapaCaseId, a.Description,
            a.AssignedToId,
            a.AssignedToId.HasValue ? userNames.GetValueOrDefault(a.AssignedToId.Value) : null,
            a.TargetDate, a.CompletionDate, a.Status, a.Notes,
            IsOverdue: a.TargetDate.HasValue && a.TargetDate < now && a.Status != CapaActionStatus.Closed)).ToList();

        var checkDtos = cas.Checks.Select(e => new EffectivenessCheckDto(
            e.Id, e.CapaCaseId, e.CheckDate,
            e.CheckedById, userNames.GetValueOrDefault(e.CheckedById),
            e.Verdict, e.Notes, e.NextCheckDate)).ToList();

        return new CapaCaseDto(
            cas.Id, cas.Title, cas.CaseNumber, cas.CapaType, cas.Status,
            cas.ProblemStatement, cas.ContainmentActions, cas.RootCauseAnalysis, cas.ProposedCorrection,
            cas.OwnerId,
            cas.OwnerId.HasValue ? userNames.GetValueOrDefault(cas.OwnerId.Value) : null,
            cas.TargetDate, cas.CompletedDate, cas.LinkedFindingId, cas.IsArchived,
            cas.CreatedAt, cas.UpdatedAt, actionDtos, checkDtos);
    }

    // ── Create ───────────────────────────────────────────────────────────────

    public async Task<CapaCaseSummaryDto> CreateAsync(
        CreateCapaCaseRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new AppValidationException("Title", "Title is required.");

        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var cas = new CapaCase
        {
            Title            = request.Title.Trim(),
            CaseNumber        = request.CaseNumber?.Trim(),
            CapaType         = request.CapaType,
            Status           = CapaCaseStatus.Open,
            ProblemStatement = request.ProblemStatement?.Trim(),
            OwnerId          = request.OwnerId,
            TargetDate       = request.TargetDate,
            LinkedFindingId  = request.LinkedFindingId
        };

        db.CapaCases.Add(cas);
        await db.SaveChangesAsync(ct);

        string? ownerName = null;
        if (cas.OwnerId.HasValue)
        {
            var owner = await users.FindByIdAsync(cas.OwnerId.Value.ToString());
            ownerName = owner?.DisplayName ?? owner?.UserName;
        }

        return new CapaCaseSummaryDto(
            cas.Id, cas.Title, cas.CaseNumber, cas.CapaType, cas.Status,
            cas.OwnerId, ownerName,
            cas.TargetDate, cas.CompletedDate, cas.LinkedFindingId, cas.IsArchived,
            0, 0, IsOverdue: false, cas.CreatedAt, cas.UpdatedAt);
    }

    // ── Update ───────────────────────────────────────────────────────────────

    public async Task UpdateAsync(Guid id, UpdateCapaCaseRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new AppValidationException("Title", "Title is required.");

        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var cas = await db.CapaCases.FindAsync([id], ct)
            ?? throw new NotFoundException(nameof(CapaCase), id);

        cas.Title             = request.Title.Trim();
        cas.CaseNumber        = request.CaseNumber?.Trim();
        cas.CapaType          = request.CapaType;
        cas.ProblemStatement  = request.ProblemStatement?.Trim();
        cas.ContainmentActions = request.ContainmentActions?.Trim();
        cas.RootCauseAnalysis = request.RootCauseAnalysis?.Trim();
        cas.ProposedCorrection = request.ProposedCorrection?.Trim();
        cas.OwnerId           = request.OwnerId;
        cas.TargetDate        = request.TargetDate;
        cas.LinkedFindingId   = request.LinkedFindingId;

        await db.SaveChangesAsync(ct);
    }

    // ── Status transition ─────────────────────────────────────────────────────

    public async Task TransitionStatusAsync(
        Guid id, TransitionCapaStatusRequest request, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var cas = await db.CapaCases.FindAsync([id], ct)
            ?? throw new NotFoundException(nameof(CapaCase), id);

        var currentIdx = Array.IndexOf(CaseWorkflow, cas.Status);
        var targetIdx  = Array.IndexOf(CaseWorkflow, request.NewStatus);

        if (targetIdx < 0 || targetIdx != currentIdx + 1)
            throw new AppValidationException("Status",
                $"Cannot transition from {cas.Status} to {request.NewStatus}.");

        cas.Status = request.NewStatus;
        if (request.NewStatus == CapaCaseStatus.Closed)
            cas.CompletedDate ??= DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);
    }

    // ── Archive toggle ────────────────────────────────────────────────────────

    public async Task<bool> ToggleArchiveAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var cas = await db.CapaCases.FindAsync([id], ct)
            ?? throw new NotFoundException(nameof(CapaCase), id);

        cas.IsArchived = !cas.IsArchived;
        await db.SaveChangesAsync(ct);
        return cas.IsArchived;
    }

    // ── Actions ───────────────────────────────────────────────────────────────

    public async Task<CapaActionDto> AddActionAsync(
        AddCapaActionRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Description))
            throw new AppValidationException("Description", "Description is required.");

        await using var db = await dbFactory.CreateDbContextAsync(ct);

        _ = await db.CapaCases.FindAsync([request.CapaCaseId], ct)
            ?? throw new NotFoundException(nameof(CapaCase), request.CapaCaseId);

        var action = new CapaAction
        {
            CapaCaseId   = request.CapaCaseId,
            Description  = request.Description.Trim(),
            AssignedToId = request.AssignedToId,
            TargetDate   = request.TargetDate,
            Status       = CapaActionStatus.Open,
            Notes        = request.Notes?.Trim()
        };

        db.CapaActions.Add(action);
        await db.SaveChangesAsync(ct);

        string? assigneeName = null;
        if (action.AssignedToId.HasValue)
        {
            var assignee = await users.FindByIdAsync(action.AssignedToId.Value.ToString());
            assigneeName = assignee?.DisplayName ?? assignee?.UserName;
        }

        return new CapaActionDto(
            action.Id, action.CapaCaseId, action.Description,
            action.AssignedToId, assigneeName,
            action.TargetDate, action.CompletionDate, action.Status, action.Notes,
            IsOverdue: false);
    }

    public async Task UpdateActionAsync(
        Guid actionId, UpdateCapaActionRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Description))
            throw new AppValidationException("Description", "Description is required.");

        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var action = await db.CapaActions.FindAsync([actionId], ct)
            ?? throw new NotFoundException(nameof(CapaAction), actionId);

        action.Description  = request.Description.Trim();
        action.AssignedToId = request.AssignedToId;
        action.TargetDate   = request.TargetDate;
        action.CompletionDate = request.CompletionDate;
        action.Status       = request.Status;
        action.Notes        = request.Notes?.Trim();

        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteActionAsync(Guid actionId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var action = await db.CapaActions.FindAsync([actionId], ct)
            ?? throw new NotFoundException(nameof(CapaAction), actionId);

        db.CapaActions.Remove(action);
        await db.SaveChangesAsync(ct);
    }

    // ── Effectiveness checks ──────────────────────────────────────────────────

    public async Task<EffectivenessCheckDto> AddEffectivenessCheckAsync(
        AddEffectivenessCheckRequest request, Guid checkedById, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var cas = await db.CapaCases.FindAsync([request.CapaCaseId], ct)
            ?? throw new NotFoundException(nameof(CapaCase), request.CapaCaseId);

        var check = new EffectivenessCheck
        {
            CapaCaseId    = request.CapaCaseId,
            CheckDate     = request.CheckDate,
            CheckedById   = checkedById,
            Verdict       = request.Verdict,
            Notes         = request.Notes?.Trim(),
            NextCheckDate = request.NextCheckDate
        };

        db.EffectivenessChecks.Add(check);

        // Auto-close if effective and at the right status
        if (request.Verdict == EffectivenessVerdict.Effective
            && cas.Status == CapaCaseStatus.EffectivenessVerification)
        {
            cas.Status        = CapaCaseStatus.Closed;
            cas.CompletedDate ??= DateTimeOffset.UtcNow;
        }

        await db.SaveChangesAsync(ct);

        var checker = await users.FindByIdAsync(checkedById.ToString());
        var checkerName = checker?.DisplayName ?? checker?.UserName ?? checkedById.ToString();

        return new EffectivenessCheckDto(
            check.Id, check.CapaCaseId, check.CheckDate,
            check.CheckedById, checkerName,
            check.Verdict, check.Notes, check.NextCheckDate);
    }
}
