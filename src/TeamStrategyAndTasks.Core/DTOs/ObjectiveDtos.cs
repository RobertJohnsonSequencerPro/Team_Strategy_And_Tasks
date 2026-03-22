using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Core.DTOs;

public record CreateObjectiveRequest(
    string Title,
    string? Description,
    string? SuccessMetric,
    string? TargetValue,
    DateTimeOffset? TargetDate);

public record UpdateObjectiveRequest(
    string Title,
    string? Description,
    string? SuccessMetric,
    string? TargetValue,
    DateTimeOffset? TargetDate,
    NodeStatus Status);
