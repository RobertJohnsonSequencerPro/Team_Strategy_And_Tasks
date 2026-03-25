namespace TeamStrategyAndTasks.Core.DTOs;

// ── CAPA DTOs ─────────────────────────────────────────────────────────────────

public record CapaCaseSummaryDto(
    Guid Id,
    string Title,
    string? CaseNumber,
    CapaType CapaType,
    CapaCaseStatus Status,
    Guid? OwnerId,
    string? OwnerName,
    DateTimeOffset? TargetDate,
    DateTimeOffset? CompletedDate,
    Guid? LinkedFindingId,
    bool IsArchived,
    int ActionCount,
    int OpenActionCount,
    bool IsOverdue,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public record CapaCaseDto(
    Guid Id,
    string Title,
    string? CaseNumber,
    CapaType CapaType,
    CapaCaseStatus Status,
    string? ProblemStatement,
    string? ContainmentActions,
    string? RootCauseAnalysis,
    string? ProposedCorrection,
    Guid? OwnerId,
    string? OwnerName,
    DateTimeOffset? TargetDate,
    DateTimeOffset? CompletedDate,
    Guid? LinkedFindingId,
    bool IsArchived,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<CapaActionDto> Actions,
    IReadOnlyList<EffectivenessCheckDto> EffectivenessChecks);

public record CapaActionDto(
    Guid Id,
    Guid CapaCaseId,
    string Description,
    Guid? AssignedToId,
    string? AssignedToName,
    DateTimeOffset? TargetDate,
    DateTimeOffset? CompletionDate,
    CapaActionStatus Status,
    string? Notes,
    bool IsOverdue);

public record EffectivenessCheckDto(
    Guid Id,
    Guid CapaCaseId,
    DateTimeOffset CheckDate,
    Guid CheckedById,
    string? CheckedByName,
    EffectivenessVerdict Verdict,
    string? Notes,
    DateTimeOffset? NextCheckDate);

// ── CAPA Request Records ──────────────────────────────────────────────────────

public record CreateCapaCaseRequest(
    string Title,
    string? CaseNumber,
    CapaType CapaType,
    string? ProblemStatement,
    Guid? OwnerId,
    DateTimeOffset? TargetDate,
    Guid? LinkedFindingId);

public record UpdateCapaCaseRequest(
    string Title,
    string? CaseNumber,
    CapaType CapaType,
    string? ProblemStatement,
    string? ContainmentActions,
    string? RootCauseAnalysis,
    string? ProposedCorrection,
    Guid? OwnerId,
    DateTimeOffset? TargetDate,
    Guid? LinkedFindingId);

public record TransitionCapaStatusRequest(
    CapaCaseStatus NewStatus);

public record AddCapaActionRequest(
    Guid CapaCaseId,
    string Description,
    Guid? AssignedToId,
    DateTimeOffset? TargetDate,
    string? Notes);

public record UpdateCapaActionRequest(
    string Description,
    Guid? AssignedToId,
    DateTimeOffset? TargetDate,
    DateTimeOffset? CompletionDate,
    CapaActionStatus Status,
    string? Notes);

public record AddEffectivenessCheckRequest(
    Guid CapaCaseId,
    DateTimeOffset CheckDate,
    EffectivenessVerdict Verdict,
    string? Notes,
    DateTimeOffset? NextCheckDate);
