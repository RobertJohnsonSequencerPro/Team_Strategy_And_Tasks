using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Core.DTOs;

// ── Summary (list page) ──────────────────────────────────────────────────────

public record ControlPlanSummaryDto(
    Guid Id,
    string Title,
    string ProcessItem,
    string? PartNumber,
    string Revision,
    ControlPlanStatus Status,
    Guid? OwnerId,
    string? OwnerName,
    Guid? LinkedPfmeaId,
    bool IsArchived,
    int CharacteristicCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

// ── Full record (detail page) ────────────────────────────────────────────────

public record ControlPlanDto(
    Guid Id,
    string Title,
    string ProcessItem,
    string? PartNumber,
    string? PartDescription,
    string Revision,
    ControlPlanStatus Status,
    Guid? OwnerId,
    string? OwnerName,
    Guid? LinkedPfmeaId,
    bool IsArchived,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<ControlPlanCharacteristicDto> Characteristics,
    IReadOnlyList<ControlPlanRevisionDto> Revisions);

public record ControlPlanCharacteristicDto(
    Guid Id,
    Guid ControlPlanId,
    int SortOrder,
    string ProcessStep,
    string? ProcessOperation,
    string? CharacteristicNo,
    CharacteristicType CharacteristicType,
    string CharacteristicDescription,
    string? SpecificationTolerance,
    string? ControlMethod,
    string? SamplingSize,
    string? SamplingFrequency,
    string? MeasurementTechnique,
    string? ReactionPlan,
    Guid? ResponsiblePersonId,
    string? ResponsiblePersonName,
    string? Notes);

public record ControlPlanRevisionDto(
    Guid Id,
    Guid ControlPlanId,
    string RevisionLabel,
    ControlPlanStatus ToStatus,
    string? Comments,
    Guid ChangedById,
    string? ChangedByName,
    DateTimeOffset ChangedAt);

// ── Request objects ──────────────────────────────────────────────────────────

public record CreateControlPlanRequest(
    string Title,
    string ProcessItem,
    string? PartNumber,
    string? PartDescription,
    string Revision,
    Guid? OwnerId,
    Guid? LinkedPfmeaId);

public record UpdateControlPlanRequest(
    string Title,
    string ProcessItem,
    string? PartNumber,
    string? PartDescription,
    string Revision,
    Guid? OwnerId,
    Guid? LinkedPfmeaId);

public record AddCharacteristicRequest(
    Guid ControlPlanId,
    int SortOrder,
    string ProcessStep,
    string? ProcessOperation,
    string? CharacteristicNo,
    CharacteristicType CharacteristicType,
    string CharacteristicDescription,
    string? SpecificationTolerance,
    string? ControlMethod,
    string? SamplingSize,
    string? SamplingFrequency,
    string? MeasurementTechnique,
    string? ReactionPlan,
    Guid? ResponsiblePersonId,
    string? Notes);

public record UpdateCharacteristicRequest(
    int SortOrder,
    string ProcessStep,
    string? ProcessOperation,
    string? CharacteristicNo,
    CharacteristicType CharacteristicType,
    string CharacteristicDescription,
    string? SpecificationTolerance,
    string? ControlMethod,
    string? SamplingSize,
    string? SamplingFrequency,
    string? MeasurementTechnique,
    string? ReactionPlan,
    Guid? ResponsiblePersonId,
    string? Notes);

public record TransitionStatusRequest(
    ControlPlanStatus NewStatus,
    string? Comments);
