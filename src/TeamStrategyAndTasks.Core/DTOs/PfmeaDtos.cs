using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Core.DTOs;

// ── Summary (list page) ──────────────────────────────────────────────────────

public record PfmeaRecordSummaryDto(
    Guid Id,
    string Title,
    string ProcessItem,
    string Revision,
    Guid? OwnerId,
    string? OwnerName,
    bool IsArchived,
    int FailureModeCount,
    int HighPriorityCount,
    int MediumPriorityCount,
    int OpenActionCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

// ── Full record (detail page) ────────────────────────────────────────────────

public record PfmeaRecordDto(
    Guid Id,
    string Title,
    string ProcessItem,
    string? Scope,
    string Revision,
    Guid? OwnerId,
    string? OwnerName,
    bool IsArchived,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<PfmeaFailureModeDto> FailureModes);

public record PfmeaFailureModeDto(
    Guid Id,
    Guid PfmeaId,
    int SortOrder,
    string ProcessStep,
    string? ProcessFunction,
    string FailureDescription,
    string? PotentialEffect,
    string? PotentialCause,
    string? CurrentControls,
    // Pre-action scores
    int Severity,
    int Occurrence,
    int Detection,
    int Rpn,
    ActionPriority Priority,
    // Post-action scores (null until re-scored)
    int? SeverityAfter,
    int? OccurrenceAfter,
    int? DetectionAfter,
    int? RpnAfter,
    Guid? AssignedToId,
    string? AssignedToName,
    string? Notes,
    IReadOnlyList<PfmeaActionDto> Actions);

public record PfmeaActionDto(
    Guid Id,
    Guid FailureModeId,
    string Description,
    Guid? AssignedToId,
    string? AssignedToName,
    DateTimeOffset? TargetDate,
    DateTimeOffset? CompletionDate,
    PfmeaActionStatus Status,
    string? OutcomeNotes,
    bool IsOverdue);

// ── Request objects ──────────────────────────────────────────────────────────

public record CreatePfmeaRecordRequest(
    string Title,
    string ProcessItem,
    string? Scope,
    string Revision,
    Guid? OwnerId);

public record UpdatePfmeaRecordRequest(
    string Title,
    string ProcessItem,
    string? Scope,
    string Revision,
    Guid? OwnerId);

public record AddFailureModeRequest(
    Guid PfmeaId,
    int SortOrder,
    string ProcessStep,
    string? ProcessFunction,
    string FailureDescription,
    string? PotentialEffect,
    string? PotentialCause,
    string? CurrentControls,
    int Severity,
    int Occurrence,
    int Detection,
    Guid? AssignedToId,
    string? Notes);

public record UpdateFailureModeRequest(
    int SortOrder,
    string ProcessStep,
    string? ProcessFunction,
    string FailureDescription,
    string? PotentialEffect,
    string? PotentialCause,
    string? CurrentControls,
    int Severity,
    int Occurrence,
    int Detection,
    int? SeverityAfter,
    int? OccurrenceAfter,
    int? DetectionAfter,
    Guid? AssignedToId,
    string? Notes);

public record AddPfmeaActionRequest(
    Guid FailureModeId,
    string Description,
    Guid? AssignedToId,
    DateTimeOffset? TargetDate);

public record UpdatePfmeaActionRequest(
    string Description,
    Guid? AssignedToId,
    DateTimeOffset? TargetDate,
    PfmeaActionStatus Status,
    string? OutcomeNotes);
