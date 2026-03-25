using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Core.DTOs;

public record QualityClauseDto(
    Guid Id,
    string ClauseNumber,
    string Title,
    string? Description,
    int SortOrder,
    bool IsActive,
    ConformanceStatus ConformanceStatus,
    Guid? AssignedToId,
    string? AssignedToName,
    DateTimeOffset? ReviewDueDate,
    string? AssessmentNotes,
    DateTimeOffset? LastReviewedAt,
    int EvidenceCount,
    bool IsOverdue);

public record UpdateClauseAssessmentRequest(
    ConformanceStatus ConformanceStatus,
    Guid? AssignedToId,
    DateTimeOffset? ReviewDueDate,
    string? Notes);

public record AddEvidenceItemRequest(
    Guid ClauseId,
    EvidenceType EvidenceType,
    string Title,
    string? Description,
    string? Url);

public record ClauseEvidenceItemDto(
    Guid Id,
    Guid ClauseId,
    EvidenceType EvidenceType,
    string Title,
    string? Description,
    string? Url,
    Guid AddedById,
    string AddedByName,
    DateTimeOffset CreatedAt);

public record ClauseReviewEventDto(
    Guid Id,
    Guid ClauseId,
    ConformanceStatus PreviousStatus,
    ConformanceStatus NewStatus,
    Guid ReviewedById,
    string ReviewedByName,
    DateTimeOffset ReviewedAt,
    string? Notes);
