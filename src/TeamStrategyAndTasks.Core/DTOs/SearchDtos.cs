namespace TeamStrategyAndTasks.Core.DTOs;

public record SearchResult(
    NodeType NodeType,
    Guid Id,
    string Title,
    string? Description,
    NodeStatus Status);
