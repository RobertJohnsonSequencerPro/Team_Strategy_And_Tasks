namespace TeamStrategyAndTasks.Core.DTOs;

// ── Audit DTOs ───────────────────────────────────────────────────────────────

public record AuditSummaryDto(
    Guid Id,
    string Title,
    AuditType AuditType,
    AuditStatus Status,
    Guid? LeadAuditorId,
    string? LeadAuditorName,
    DateTimeOffset? ScheduledDate,
    DateTimeOffset? CompletedDate,
    bool IsArchived,
    int FindingCount,
    int OpenFindingCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public record AuditDto(
    Guid Id,
    string Title,
    AuditType AuditType,
    AuditStatus Status,
    string? Scope,
    Guid? LeadAuditorId,
    string? LeadAuditorName,
    DateTimeOffset? ScheduledDate,
    DateTimeOffset? StartDate,
    DateTimeOffset? CompletedDate,
    string? Notes,
    bool IsArchived,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<AuditFindingDto> Findings);

public record AuditFindingDto(
    Guid Id,
    Guid AuditId,
    string Title,
    string? Description,
    FindingType FindingType,
    FindingSeverity Severity,
    string? ClauseReference,
    AuditFindingStatus Status,
    Guid? AssignedToId,
    string? AssignedToName,
    DateTimeOffset? DueDate,
    string? ContainmentNotes,
    string? RootCauseNotes,
    string? CorrectiveActionNotes,
    string? Notes,
    Guid? LinkedCapaCaseId,
    bool IsOverdue);

// ── Audit Request Records ─────────────────────────────────────────────────────

public record CreateAuditRequest(
    string Title,
    AuditType AuditType,
    string? Scope,
    Guid? LeadAuditorId,
    DateTimeOffset? ScheduledDate,
    string? Notes);

public record UpdateAuditRequest(
    string Title,
    AuditType AuditType,
    string? Scope,
    Guid? LeadAuditorId,
    DateTimeOffset? ScheduledDate,
    DateTimeOffset? StartDate,
    DateTimeOffset? CompletedDate,
    string? Notes);

public record TransitionAuditStatusRequest(
    AuditStatus NewStatus);

public record AddFindingRequest(
    Guid AuditId,
    string Title,
    string? Description,
    FindingType FindingType,
    FindingSeverity Severity,
    string? ClauseReference,
    Guid? AssignedToId,
    DateTimeOffset? DueDate,
    string? Notes);

public record UpdateFindingRequest(
    string Title,
    string? Description,
    FindingType FindingType,
    FindingSeverity Severity,
    string? ClauseReference,
    Guid? AssignedToId,
    DateTimeOffset? DueDate,
    string? ContainmentNotes,
    string? RootCauseNotes,
    string? CorrectiveActionNotes,
    string? Notes,
    Guid? LinkedCapaCaseId);

public record TransitionFindingStatusRequest(
    AuditFindingStatus NewStatus);
