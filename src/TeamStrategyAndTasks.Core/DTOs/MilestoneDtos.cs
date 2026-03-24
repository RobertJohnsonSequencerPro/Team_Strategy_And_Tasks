using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Core.DTOs;

public record AddMilestoneRequest(
    Guid           InitiativeId,
    string         Title,
    DateTimeOffset DueDate,
    Guid           OwnerId,
    string?        Notes = null);

public record UpdateMilestoneRequest(
    string         Title,
    DateTimeOffset DueDate,
    Guid           OwnerId,
    string?        Notes);

public record MilestoneDto(
    Guid            Id,
    Guid            InitiativeId,
    string          Title,
    DateTimeOffset  DueDate,
    MilestoneStatus Status,
    DateTimeOffset? CompletedAt,
    string?         Notes,
    Guid            OwnerId,
    string          OwnerName,
    bool            IsOverdue);
