using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Core.DTOs;

public record CreateTaskRequest(
    string Title,
    string? Description,
    Guid? AssigneeId,
    decimal? EstimatedEffort,
    DateTimeOffset? TargetDate);

public record UpdateTaskRequest(
    string Title,
    string? Description,
    Guid? AssigneeId,
    decimal? EstimatedEffort,
    decimal? ActualEffort,
    DateTimeOffset? TargetDate,
    NodeStatus Status);
