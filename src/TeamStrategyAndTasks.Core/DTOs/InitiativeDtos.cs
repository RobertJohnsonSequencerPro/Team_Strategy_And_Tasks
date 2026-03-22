using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Core.DTOs;

public record CreateInitiativeRequest(
    string Title,
    string? Description,
    DateTimeOffset? TargetDate);

public record UpdateInitiativeRequest(
    string Title,
    string? Description,
    DateTimeOffset? TargetDate,
    NodeStatus Status);
