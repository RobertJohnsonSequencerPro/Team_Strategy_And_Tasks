namespace TeamStrategyAndTasks.Core.DTOs;

// ── RCA DTOs ─────────────────────────────────────────────────────────────────

public record RcaCaseSummaryDto(
    Guid Id,
    string Title,
    RcaType RcaType,
    RcaCaseStatus Status,
    string? ProcessArea,
    string? PartFamily,
    string? RootCauseSummary,
    Guid? LinkedCapaCaseId,
    Guid? LinkedFindingId,
    Guid? InitiatedById,
    string? InitiatedByName,
    Guid? ApprovedById,
    string? ApprovedByName,
    DateTimeOffset? ApprovedAt,
    bool IsArchived,
    int NodeCount,
    IReadOnlyList<string> Tags,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public record RcaCaseDto(
    Guid Id,
    string Title,
    string? ProblemStatement,
    RcaType RcaType,
    RcaCaseStatus Status,
    string? ProcessArea,
    string? PartFamily,
    string? RootCauseSummary,
    Guid? LinkedCapaCaseId,
    Guid? LinkedFindingId,
    Guid? InitiatedById,
    string? InitiatedByName,
    Guid? ApprovedById,
    string? ApprovedByName,
    DateTimeOffset? ApprovedAt,
    bool IsArchived,
    IReadOnlyList<string> Tags,
    IReadOnlyList<FiveWhyNodeDto> FiveWhyNodes,
    IReadOnlyList<IshikawaCauseDto> IshikawaCauses,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public record FiveWhyNodeDto(
    Guid Id,
    Guid RcaCaseId,
    Guid? ParentId,
    int DisplayOrder,
    string WhyQuestion,
    string? BecauseAnswer,
    bool IsRootCause);

public record IshikawaCauseDto(
    Guid Id,
    Guid RcaCaseId,
    IshikawaCauseCategory Category,
    Guid? ParentCauseId,
    int DisplayOrder,
    string CauseText,
    bool IsRootCause);

public record RecurringRootCauseDto(
    string RootCauseText,
    int CaseCount,
    IReadOnlyList<RcaCaseSummaryDto> Cases);

// ── RCA Request Records ───────────────────────────────────────────────────────

public record CreateRcaCaseRequest(
    string Title,
    string? ProblemStatement,
    RcaType RcaType,
    string? ProcessArea,
    string? PartFamily,
    Guid? LinkedCapaCaseId,
    Guid? LinkedFindingId,
    Guid? InitiatedById);

public record UpdateRcaCaseRequest(
    string Title,
    string? ProblemStatement,
    string? ProcessArea,
    string? PartFamily,
    string? RootCauseSummary,
    Guid? LinkedCapaCaseId,
    Guid? LinkedFindingId);

public record TransitionRcaStatusRequest(
    RcaCaseStatus NewStatus);

public record SetRcaTagsRequest(
    IReadOnlyList<string> Tags);

public record AddFiveWhyNodeRequest(
    Guid RcaCaseId,
    Guid? ParentId,
    int DisplayOrder,
    string WhyQuestion,
    string? BecauseAnswer = null,
    bool IsRootCause = false);

public record UpdateFiveWhyNodeRequest(
    string WhyQuestion,
    string? BecauseAnswer,
    bool IsRootCause);

public record AddIshikawaCauseRequest(
    Guid RcaCaseId,
    IshikawaCauseCategory Category,
    Guid? ParentCauseId,
    int DisplayOrder,
    string CauseText,
    bool IsRootCause = false);

public record UpdateIshikawaCauseRequest(
    IshikawaCauseCategory Category,
    string CauseText,
    bool IsRootCause);
