using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Core.DTOs;

public record CreateProcessRequest(
    string Title,
    string? Description,
    string? SuccessMetric,
    string? TargetValue,
    DateTimeOffset? TargetDate);

public record UpdateProcessRequest(
    string Title,
    string? Description,
    string? SuccessMetric,
    string? TargetValue,
    DateTimeOffset? TargetDate,
    NodeStatus Status);
