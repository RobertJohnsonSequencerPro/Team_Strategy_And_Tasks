using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Core.DTOs;

public record CreateTaskStepRequest(
    string Title,
    string? Description,
    Guid? AssigneeId,
    DateTimeOffset? DueDate,
    int DisplayOrder);

public record UpdateTaskStepRequest(
    string Title,
    string? Description,
    Guid? AssigneeId,
    DateTimeOffset? DueDate,
    int DisplayOrder,
    NodeStatus Status);
