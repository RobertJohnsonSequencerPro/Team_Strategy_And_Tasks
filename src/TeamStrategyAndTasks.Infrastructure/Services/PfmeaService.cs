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

public class PfmeaService(
    IDbContextFactory<AppDbContext> dbFactory,
    UserManager<ApplicationUser> users) : IPfmeaService
{
    // ── Static helpers ───────────────────────────────────────────────────────

    private static int ComputeRpn(int s, int o, int d) => s * o * d;

    private static ActionPriority ComputePriority(int s, int o, int d)
    {
        var rpn = s * o * d;
        if (rpn >= 200 || s >= 9) return ActionPriority.High;
        if (rpn >= 100 || (s >= 7 && o >= 4)) return ActionPriority.Medium;
        return ActionPriority.Low;
    }

    private static void ValidateScore(int value, string name)
    {
        if (value < 1 || value > 10)
            throw new AppValidationException(name, $"{name} must be between 1 and 10.");
    }

    // ── Records ──────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<PfmeaRecordSummaryDto>> GetAllAsync(
        bool showArchived = false, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var records = await db.PfmeaRecords
            .AsNoTracking()
            .Where(r => showArchived || !r.IsArchived)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new
            {
                r.Id, r.Title, r.ProcessItem, r.Revision, r.OwnerId, r.IsArchived,
                r.CreatedAt, r.UpdatedAt,
                FailureModes = r.FailureModes.Select(fm => new
                {
                    fm.Severity, fm.Occurrence, fm.Detection,
                    OpenActions = fm.Actions.Count(a => a.Status != PfmeaActionStatus.Closed)
                }).ToList()
            })
            .ToListAsync(ct);

        if (records.Count == 0) return [];

        var ownerIds = records
            .Where(r => r.OwnerId.HasValue)
            .Select(r => r.OwnerId!.Value)
            .Distinct()
            .ToList();

        var ownerNames = ownerIds.Count > 0
            ? await db.Users.AsNoTracking()
                .Where(u => ownerIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.DisplayName ?? u.UserName ?? u.Id.ToString(), ct)
            : new Dictionary<Guid, string>();

        return records.Select(r =>
        {
            var highCount   = r.FailureModes.Count(fm => ComputePriority(fm.Severity, fm.Occurrence, fm.Detection) == ActionPriority.High);
            var medCount    = r.FailureModes.Count(fm => ComputePriority(fm.Severity, fm.Occurrence, fm.Detection) == ActionPriority.Medium);
            var openActions = r.FailureModes.Sum(fm => fm.OpenActions);

            return new PfmeaRecordSummaryDto(
                r.Id, r.Title, r.ProcessItem, r.Revision,
                r.OwnerId, r.OwnerId.HasValue ? ownerNames.GetValueOrDefault(r.OwnerId.Value) : null,
                r.IsArchived, r.FailureModes.Count, highCount, medCount, openActions,
                r.CreatedAt, r.UpdatedAt);
        }).ToList();
    }

    public async Task<PfmeaRecordDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var record = await db.PfmeaRecords
            .AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new
            {
                r.Id, r.Title, r.ProcessItem, r.Scope, r.Revision,
                r.OwnerId, r.IsArchived, r.CreatedAt, r.UpdatedAt,
                FailureModes = r.FailureModes
                    .OrderBy(fm => fm.SortOrder)
                    .Select(fm => new
                    {
                        fm.Id, fm.PfmeaId, fm.SortOrder,
                        fm.ProcessStep, fm.ProcessFunction, fm.FailureDescription,
                        fm.PotentialEffect, fm.PotentialCause, fm.CurrentControls,
                        fm.Severity, fm.Occurrence, fm.Detection,
                        fm.SeverityAfter, fm.OccurrenceAfter, fm.DetectionAfter,
                        fm.AssignedToId, fm.Notes,
                        Actions = fm.Actions
                            .OrderBy(a => a.CreatedAt)
                            .Select(a => new
                            {
                                a.Id, a.FailureModeId, a.Description,
                                a.AssignedToId, a.TargetDate, a.CompletionDate,
                                a.Status, a.OutcomeNotes
                            }).ToList()
                    }).ToList()
            })
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(PfmeaRecord), id);

        // Collect all user IDs for a single bulk name resolution
        var allUserIds = new HashSet<Guid>();
        if (record.OwnerId.HasValue) allUserIds.Add(record.OwnerId.Value);
        foreach (var fm in record.FailureModes)
        {
            if (fm.AssignedToId.HasValue) allUserIds.Add(fm.AssignedToId.Value);
            foreach (var a in fm.Actions)
                if (a.AssignedToId.HasValue) allUserIds.Add(a.AssignedToId.Value);
        }

        var userNames = allUserIds.Count > 0
            ? await db.Users.AsNoTracking()
                .Where(u => allUserIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.DisplayName ?? u.UserName ?? u.Id.ToString(), ct)
            : new Dictionary<Guid, string>();

        var now = DateTimeOffset.UtcNow;

        var fmDtos = record.FailureModes.Select(fm =>
        {
            var rpn      = ComputeRpn(fm.Severity, fm.Occurrence, fm.Detection);
            var priority = ComputePriority(fm.Severity, fm.Occurrence, fm.Detection);
            int? rpnAfter = fm.SeverityAfter.HasValue && fm.OccurrenceAfter.HasValue && fm.DetectionAfter.HasValue
                ? ComputeRpn(fm.SeverityAfter.Value, fm.OccurrenceAfter.Value, fm.DetectionAfter.Value)
                : null;

            var actions = fm.Actions.Select(a => new PfmeaActionDto(
                a.Id, a.FailureModeId, a.Description,
                a.AssignedToId,
                a.AssignedToId.HasValue ? userNames.GetValueOrDefault(a.AssignedToId.Value) : null,
                a.TargetDate, a.CompletionDate, a.Status, a.OutcomeNotes,
                IsOverdue: a.TargetDate.HasValue && a.TargetDate.Value < now
                           && a.Status != PfmeaActionStatus.Closed))
                .ToList();

            return new PfmeaFailureModeDto(
                fm.Id, fm.PfmeaId, fm.SortOrder,
                fm.ProcessStep, fm.ProcessFunction, fm.FailureDescription,
                fm.PotentialEffect, fm.PotentialCause, fm.CurrentControls,
                fm.Severity, fm.Occurrence, fm.Detection, rpn, priority,
                fm.SeverityAfter, fm.OccurrenceAfter, fm.DetectionAfter, rpnAfter,
                fm.AssignedToId,
                fm.AssignedToId.HasValue ? userNames.GetValueOrDefault(fm.AssignedToId.Value) : null,
                fm.Notes, actions);
        }).ToList();

        return new PfmeaRecordDto(
            record.Id, record.Title, record.ProcessItem, record.Scope, record.Revision,
            record.OwnerId,
            record.OwnerId.HasValue ? userNames.GetValueOrDefault(record.OwnerId.Value) : null,
            record.IsArchived, record.CreatedAt, record.UpdatedAt, fmDtos);
    }

    public async Task<PfmeaRecordSummaryDto> CreateAsync(
        CreatePfmeaRecordRequest request, Guid userId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new AppValidationException("Title", "Title is required.");
        if (string.IsNullOrWhiteSpace(request.ProcessItem))
            throw new AppValidationException("ProcessItem", "Process/Item is required.");

        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var record = new PfmeaRecord
        {
            Title       = request.Title.Trim(),
            ProcessItem = request.ProcessItem.Trim(),
            Scope       = request.Scope?.Trim(),
            Revision    = string.IsNullOrWhiteSpace(request.Revision) ? "Rev A" : request.Revision.Trim(),
            OwnerId     = request.OwnerId
        };

        db.PfmeaRecords.Add(record);
        await db.SaveChangesAsync(ct);

        string? ownerName = null;
        if (record.OwnerId.HasValue)
        {
            var owner = await users.FindByIdAsync(record.OwnerId.Value.ToString());
            ownerName = owner?.DisplayName ?? owner?.UserName;
        }

        return new PfmeaRecordSummaryDto(
            record.Id, record.Title, record.ProcessItem, record.Revision,
            record.OwnerId, ownerName, record.IsArchived,
            0, 0, 0, 0, record.CreatedAt, record.UpdatedAt);
    }

    public async Task UpdateAsync(
        Guid id, UpdatePfmeaRecordRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new AppValidationException("Title", "Title is required.");
        if (string.IsNullOrWhiteSpace(request.ProcessItem))
            throw new AppValidationException("ProcessItem", "Process/Item is required.");

        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var record = await db.PfmeaRecords.FindAsync([id], ct)
            ?? throw new NotFoundException(nameof(PfmeaRecord), id);

        record.Title       = request.Title.Trim();
        record.ProcessItem = request.ProcessItem.Trim();
        record.Scope       = request.Scope?.Trim();
        record.Revision    = string.IsNullOrWhiteSpace(request.Revision) ? "Rev A" : request.Revision.Trim();
        record.OwnerId     = request.OwnerId;

        await db.SaveChangesAsync(ct);
    }

    public async Task<bool> ToggleArchiveAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var record = await db.PfmeaRecords.FindAsync([id], ct)
            ?? throw new NotFoundException(nameof(PfmeaRecord), id);

        record.IsArchived = !record.IsArchived;
        await db.SaveChangesAsync(ct);
        return record.IsArchived;
    }

    // ── Failure Modes ────────────────────────────────────────────────────────

    public async Task<PfmeaFailureModeDto> AddFailureModeAsync(
        AddFailureModeRequest request, CancellationToken ct = default)
    {
        ValidateScore(request.Severity, "Severity");
        ValidateScore(request.Occurrence, "Occurrence");
        ValidateScore(request.Detection, "Detection");

        if (string.IsNullOrWhiteSpace(request.ProcessStep))
            throw new AppValidationException("ProcessStep", "Process step is required.");
        if (string.IsNullOrWhiteSpace(request.FailureDescription))
            throw new AppValidationException("FailureDescription", "Failure description is required.");

        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var fm = new PfmeaFailureMode
        {
            PfmeaId            = request.PfmeaId,
            SortOrder          = request.SortOrder,
            ProcessStep        = request.ProcessStep.Trim(),
            ProcessFunction    = request.ProcessFunction?.Trim(),
            FailureDescription = request.FailureDescription.Trim(),
            PotentialEffect    = request.PotentialEffect?.Trim(),
            PotentialCause     = request.PotentialCause?.Trim(),
            CurrentControls    = request.CurrentControls?.Trim(),
            Severity           = request.Severity,
            Occurrence         = request.Occurrence,
            Detection          = request.Detection,
            AssignedToId       = request.AssignedToId,
            Notes              = request.Notes?.Trim()
        };

        db.PfmeaFailureModes.Add(fm);
        await db.SaveChangesAsync(ct);

        string? assigneeName = null;
        if (fm.AssignedToId.HasValue)
        {
            var u = await users.FindByIdAsync(fm.AssignedToId.Value.ToString());
            assigneeName = u?.DisplayName ?? u?.UserName;
        }

        return new PfmeaFailureModeDto(
            fm.Id, fm.PfmeaId, fm.SortOrder,
            fm.ProcessStep, fm.ProcessFunction, fm.FailureDescription,
            fm.PotentialEffect, fm.PotentialCause, fm.CurrentControls,
            fm.Severity, fm.Occurrence, fm.Detection,
            ComputeRpn(fm.Severity, fm.Occurrence, fm.Detection),
            ComputePriority(fm.Severity, fm.Occurrence, fm.Detection),
            null, null, null, null,
            fm.AssignedToId, assigneeName, fm.Notes, []);
    }

    public async Task UpdateFailureModeAsync(
        Guid failureModeId, UpdateFailureModeRequest request, CancellationToken ct = default)
    {
        ValidateScore(request.Severity, "Severity");
        ValidateScore(request.Occurrence, "Occurrence");
        ValidateScore(request.Detection, "Detection");

        if (request.SeverityAfter.HasValue)   ValidateScore(request.SeverityAfter.Value,   "Severity (after)");
        if (request.OccurrenceAfter.HasValue) ValidateScore(request.OccurrenceAfter.Value, "Occurrence (after)");
        if (request.DetectionAfter.HasValue)  ValidateScore(request.DetectionAfter.Value,  "Detection (after)");

        if (string.IsNullOrWhiteSpace(request.ProcessStep))
            throw new AppValidationException("ProcessStep", "Process step is required.");
        if (string.IsNullOrWhiteSpace(request.FailureDescription))
            throw new AppValidationException("FailureDescription", "Failure description is required.");

        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var fm = await db.PfmeaFailureModes.FindAsync([failureModeId], ct)
            ?? throw new NotFoundException(nameof(PfmeaFailureMode), failureModeId);

        fm.SortOrder          = request.SortOrder;
        fm.ProcessStep        = request.ProcessStep.Trim();
        fm.ProcessFunction    = request.ProcessFunction?.Trim();
        fm.FailureDescription = request.FailureDescription.Trim();
        fm.PotentialEffect    = request.PotentialEffect?.Trim();
        fm.PotentialCause     = request.PotentialCause?.Trim();
        fm.CurrentControls    = request.CurrentControls?.Trim();
        fm.Severity           = request.Severity;
        fm.Occurrence         = request.Occurrence;
        fm.Detection          = request.Detection;
        fm.SeverityAfter      = request.SeverityAfter;
        fm.OccurrenceAfter    = request.OccurrenceAfter;
        fm.DetectionAfter     = request.DetectionAfter;
        fm.AssignedToId       = request.AssignedToId;
        fm.Notes              = request.Notes?.Trim();

        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteFailureModeAsync(Guid failureModeId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var fm = await db.PfmeaFailureModes.FindAsync([failureModeId], ct)
            ?? throw new NotFoundException(nameof(PfmeaFailureMode), failureModeId);

        db.PfmeaFailureModes.Remove(fm);
        await db.SaveChangesAsync(ct);
    }

    // ── Actions ──────────────────────────────────────────────────────────────

    public async Task<PfmeaActionDto> AddActionAsync(
        AddPfmeaActionRequest request, Guid userId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Description))
            throw new AppValidationException("Description", "Description is required.");

        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var action = new PfmeaAction
        {
            FailureModeId = request.FailureModeId,
            Description   = request.Description.Trim(),
            AssignedToId  = request.AssignedToId,
            TargetDate    = request.TargetDate,
            Status        = PfmeaActionStatus.Open,
            CreatedById   = userId
        };

        db.PfmeaActions.Add(action);
        await db.SaveChangesAsync(ct);

        string? assigneeName = null;
        if (action.AssignedToId.HasValue)
        {
            var u = await users.FindByIdAsync(action.AssignedToId.Value.ToString());
            assigneeName = u?.DisplayName ?? u?.UserName;
        }

        var now = DateTimeOffset.UtcNow;
        return new PfmeaActionDto(
            action.Id, action.FailureModeId, action.Description,
            action.AssignedToId, assigneeName,
            action.TargetDate, action.CompletionDate, action.Status, action.OutcomeNotes,
            IsOverdue: action.TargetDate.HasValue && action.TargetDate.Value < now
                       && action.Status != PfmeaActionStatus.Closed);
    }

    public async Task UpdateActionAsync(
        Guid actionId, UpdatePfmeaActionRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Description))
            throw new AppValidationException("Description", "Description is required.");

        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var action = await db.PfmeaActions.FindAsync([actionId], ct)
            ?? throw new NotFoundException(nameof(PfmeaAction), actionId);

        action.Description  = request.Description.Trim();
        action.AssignedToId = request.AssignedToId;
        action.TargetDate   = request.TargetDate;
        action.OutcomeNotes = request.OutcomeNotes?.Trim();

        var wasOpen = action.Status != PfmeaActionStatus.Closed;
        action.Status = request.Status;

        if (wasOpen && request.Status == PfmeaActionStatus.Closed)
            action.CompletionDate = DateTimeOffset.UtcNow;
        else if (request.Status != PfmeaActionStatus.Closed)
            action.CompletionDate = null;

        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteActionAsync(Guid actionId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var action = await db.PfmeaActions.FindAsync([actionId], ct)
            ?? throw new NotFoundException(nameof(PfmeaAction), actionId);

        db.PfmeaActions.Remove(action);
        await db.SaveChangesAsync(ct);
    }
}
