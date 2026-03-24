using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Core.DTOs;

/// <summary>Payload for creating a new Key Result on an Objective.</summary>
public record AddKeyResultRequest(
    Guid   ObjectiveId,
    string Title,
    decimal TargetValue,
    string Unit,
    Guid   OwnerId,
    decimal CurrentValue = 0m);

/// <summary>Payload for recording a progress update or editing a Key Result.</summary>
public record UpdateKeyResultRequest(
    string  Title,
    decimal CurrentValue,
    decimal TargetValue,
    string  Unit,
    Guid    OwnerId);

/// <summary>Read model returned by the service.</summary>
public record KeyResultDto(
    Guid            Id,
    Guid            ObjectiveId,
    string          Title,
    decimal         CurrentValue,
    decimal         TargetValue,
    string          Unit,
    Guid            OwnerId,
    string          OwnerName,
    double          ProgressPercent,
    KeyResultStatus Status,
    DateTimeOffset  UpdatedAt);
