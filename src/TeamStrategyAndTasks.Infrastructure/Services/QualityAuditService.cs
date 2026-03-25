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

public class QualityAuditService(
    IDbContextFactory<AppDbContext> dbFactory,
    UserManager<ApplicationUser> users) : IQualityAuditService
{
    private static readonly AuditFindingStatus[] FindingWorkflow =
    [
        AuditFindingStatus.Open,
        AuditFindingStatus.Containment,
        AuditFindingStatus.RootCauseAnalysis,
        AuditFindingStatus.CorrectiveAction,
        AuditFindingStatus.EffectivenessVerification,
        AuditFindingStatus.Closed
    ];

    // ── List ─────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<AuditSummaryDto>> GetAllAsync(
        bool showArchived = false, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var audits = await db.Audits
            .AsNoTracking()
            .Where(a => showArchived || !a.IsArchived)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new
            {
                a.Id, a.Title, a.AuditType, a.Status,
                a.LeadAuditorId, a.ScheduledDate, a.CompletedDate,
                a.IsArchived, a.CreatedAt, a.UpdatedAt,
                FindingCount     = a.Findings.Count,
                OpenFindingCount = a.Findings.Count(f => f.Status != AuditFindingStatus.Closed)
            })
            .ToListAsync(ct);

        if (audits.Count == 0) return [];

        var auditorIds = audits
            .Where(a => a.LeadAuditorId.HasValue)
            .Select(a => a.LeadAuditorId!.Value)
            .Distinct()
            .ToList();

        var auditorNames = auditorIds.Count > 0
            ? await db.Users.AsNoTracking()
                .Where(u => auditorIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.DisplayName ?? u.UserName ?? u.Id.ToString(), ct)
            : new Dictionary<Guid, string>();

        return audits.Select(a => new AuditSummaryDto(
            a.Id, a.Title, a.AuditType, a.Status,
            a.LeadAuditorId,
            a.LeadAuditorId.HasValue ? auditorNames.GetValueOrDefault(a.LeadAuditorId.Value) : null,
            a.ScheduledDate, a.CompletedDate, a.IsArchived,
            a.FindingCount, a.OpenFindingCount,
            a.CreatedAt, a.UpdatedAt)).ToList();
    }

    // ── Detail ───────────────────────────────────────────────────────────────

    public async Task<AuditDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var audit = await db.Audits
            .AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => new
            {
                a.Id, a.Title, a.AuditType, a.Status, a.Scope,
                a.LeadAuditorId, a.ScheduledDate, a.StartDate, a.CompletedDate,
                a.Notes, a.IsArchived, a.CreatedAt, a.UpdatedAt,
                Findings = a.Findings
                    .OrderBy(f => f.CreatedAt)
                    .Select(f => new
                    {
                        f.Id, f.AuditId, f.Title, f.Description,
                        f.FindingType, f.Severity, f.ClauseReference, f.Status,
                        f.AssignedToId, f.DueDate,
                        f.ContainmentNotes, f.RootCauseNotes, f.CorrectiveActionNotes,
                        f.Notes, f.LinkedCapaCaseId, f.CreatedAt
                    }).ToList()
            })
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Audit), id);

        var allUserIds = new HashSet<Guid>();
        if (audit.LeadAuditorId.HasValue) allUserIds.Add(audit.LeadAuditorId.Value);
        foreach (var f in audit.Findings)
            if (f.AssignedToId.HasValue) allUserIds.Add(f.AssignedToId.Value);

        var userNames = allUserIds.Count > 0
            ? await db.Users.AsNoTracking()
                .Where(u => allUserIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.DisplayName ?? u.UserName ?? u.Id.ToString(), ct)
            : new Dictionary<Guid, string>();

        var now = DateTimeOffset.UtcNow;

        var findingDtos = audit.Findings.Select(f => new AuditFindingDto(
            f.Id, f.AuditId, f.Title, f.Description,
            f.FindingType, f.Severity, f.ClauseReference, f.Status,
            f.AssignedToId,
            f.AssignedToId.HasValue ? userNames.GetValueOrDefault(f.AssignedToId.Value) : null,
            f.DueDate,
            f.ContainmentNotes, f.RootCauseNotes, f.CorrectiveActionNotes, f.Notes,
            f.LinkedCapaCaseId,
            IsOverdue: f.DueDate.HasValue && f.DueDate < now && f.Status != AuditFindingStatus.Closed)).ToList();

        return new AuditDto(
            audit.Id, audit.Title, audit.AuditType, audit.Status, audit.Scope,
            audit.LeadAuditorId,
            audit.LeadAuditorId.HasValue ? userNames.GetValueOrDefault(audit.LeadAuditorId.Value) : null,
            audit.ScheduledDate, audit.StartDate, audit.CompletedDate,
            audit.Notes, audit.IsArchived, audit.CreatedAt, audit.UpdatedAt,
            findingDtos);
    }

    // ── Create ───────────────────────────────────────────────────────────────

    public async Task<AuditSummaryDto> CreateAsync(
        CreateAuditRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new AppValidationException("Title", "Title is required.");

        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var audit = new Audit
        {
            Title         = request.Title.Trim(),
            AuditType     = request.AuditType,
            Status        = AuditStatus.Planned,
            Scope         = request.Scope?.Trim(),
            LeadAuditorId = request.LeadAuditorId,
            ScheduledDate = request.ScheduledDate,
            Notes         = request.Notes?.Trim()
        };

        db.Audits.Add(audit);
        await db.SaveChangesAsync(ct);

        string? auditorName = null;
        if (audit.LeadAuditorId.HasValue)
        {
            var auditor = await users.FindByIdAsync(audit.LeadAuditorId.Value.ToString());
            auditorName = auditor?.DisplayName ?? auditor?.UserName;
        }

        return new AuditSummaryDto(
            audit.Id, audit.Title, audit.AuditType, audit.Status,
            audit.LeadAuditorId, auditorName,
            audit.ScheduledDate, audit.CompletedDate,
            audit.IsArchived, 0, 0, audit.CreatedAt, audit.UpdatedAt);
    }

    // ── Update ───────────────────────────────────────────────────────────────

    public async Task UpdateAsync(Guid id, UpdateAuditRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new AppValidationException("Title", "Title is required.");

        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var audit = await db.Audits.FindAsync([id], ct)
            ?? throw new NotFoundException(nameof(Audit), id);

        audit.Title         = request.Title.Trim();
        audit.AuditType     = request.AuditType;
        audit.Scope         = request.Scope?.Trim();
        audit.LeadAuditorId = request.LeadAuditorId;
        audit.ScheduledDate = request.ScheduledDate;
        audit.StartDate     = request.StartDate;
        audit.CompletedDate = request.CompletedDate;
        audit.Notes         = request.Notes?.Trim();

        await db.SaveChangesAsync(ct);
    }

    // ── Status transition ────────────────────────────────────────────────────

    public async Task TransitionStatusAsync(
        Guid id, TransitionAuditStatusRequest request, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var audit = await db.Audits.FindAsync([id], ct)
            ?? throw new NotFoundException(nameof(Audit), id);

        var valid = audit.Status switch
        {
            AuditStatus.Planned    => new[] { AuditStatus.InProgress, AuditStatus.Cancelled },
            AuditStatus.InProgress => new[] { AuditStatus.Completed, AuditStatus.Cancelled },
            _                      => Array.Empty<AuditStatus>()
        };

        if (!valid.Contains(request.NewStatus))
            throw new AppValidationException("Status",
                $"Cannot transition from {audit.Status} to {request.NewStatus}.");

        audit.Status = request.NewStatus;
        if (request.NewStatus == AuditStatus.InProgress && audit.StartDate is null)
            audit.StartDate = DateTimeOffset.UtcNow;
        if (request.NewStatus == AuditStatus.Completed)
            audit.CompletedDate ??= DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);
    }

    // ── Archive toggle ────────────────────────────────────────────────────────

    public async Task<bool> ToggleArchiveAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var audit = await db.Audits.FindAsync([id], ct)
            ?? throw new NotFoundException(nameof(Audit), id);

        audit.IsArchived = !audit.IsArchived;
        await db.SaveChangesAsync(ct);
        return audit.IsArchived;
    }

    // ── Findings ──────────────────────────────────────────────────────────────

    public async Task<AuditFindingDto> AddFindingAsync(
        AddFindingRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new AppValidationException("Title", "Title is required.");

        await using var db = await dbFactory.CreateDbContextAsync(ct);

        _ = await db.Audits.FindAsync([request.AuditId], ct)
            ?? throw new NotFoundException(nameof(Audit), request.AuditId);

        var finding = new AuditFinding
        {
            AuditId         = request.AuditId,
            Title           = request.Title.Trim(),
            Description     = request.Description?.Trim(),
            FindingType     = request.FindingType,
            Severity        = request.Severity,
            ClauseReference = request.ClauseReference?.Trim(),
            Status          = AuditFindingStatus.Open,
            AssignedToId    = request.AssignedToId,
            DueDate         = request.DueDate,
            Notes           = request.Notes?.Trim()
        };

        db.AuditFindings.Add(finding);
        await db.SaveChangesAsync(ct);

        string? assigneeName = null;
        if (finding.AssignedToId.HasValue)
        {
            var assignee = await users.FindByIdAsync(finding.AssignedToId.Value.ToString());
            assigneeName = assignee?.DisplayName ?? assignee?.UserName;
        }

        return new AuditFindingDto(
            finding.Id, finding.AuditId, finding.Title, finding.Description,
            finding.FindingType, finding.Severity, finding.ClauseReference, finding.Status,
            finding.AssignedToId, assigneeName,
            finding.DueDate, finding.ContainmentNotes, finding.RootCauseNotes,
            finding.CorrectiveActionNotes, finding.Notes,
            finding.LinkedCapaCaseId, IsOverdue: false);
    }

    public async Task UpdateFindingAsync(
        Guid findingId, UpdateFindingRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new AppValidationException("Title", "Title is required.");

        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var finding = await db.AuditFindings.FindAsync([findingId], ct)
            ?? throw new NotFoundException(nameof(AuditFinding), findingId);

        finding.Title                = request.Title.Trim();
        finding.Description          = request.Description?.Trim();
        finding.FindingType          = request.FindingType;
        finding.Severity             = request.Severity;
        finding.ClauseReference      = request.ClauseReference?.Trim();
        finding.AssignedToId         = request.AssignedToId;
        finding.DueDate              = request.DueDate;
        finding.ContainmentNotes     = request.ContainmentNotes?.Trim();
        finding.RootCauseNotes       = request.RootCauseNotes?.Trim();
        finding.CorrectiveActionNotes = request.CorrectiveActionNotes?.Trim();
        finding.Notes                = request.Notes?.Trim();
        finding.LinkedCapaCaseId     = request.LinkedCapaCaseId;

        await db.SaveChangesAsync(ct);
    }

    public async Task TransitionFindingStatusAsync(
        Guid findingId, TransitionFindingStatusRequest request, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var finding = await db.AuditFindings.FindAsync([findingId], ct)
            ?? throw new NotFoundException(nameof(AuditFinding), findingId);

        var currentIdx = Array.IndexOf(FindingWorkflow, finding.Status);
        var targetIdx  = Array.IndexOf(FindingWorkflow, request.NewStatus);

        if (targetIdx < 0 || targetIdx != currentIdx + 1)
            throw new AppValidationException("Status",
                $"Cannot transition from {finding.Status} to {request.NewStatus}.");

        finding.Status = request.NewStatus;
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteFindingAsync(Guid findingId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var finding = await db.AuditFindings.FindAsync([findingId], ct)
            ?? throw new NotFoundException(nameof(AuditFinding), findingId);

        db.AuditFindings.Remove(finding);
        await db.SaveChangesAsync(ct);
    }
}
