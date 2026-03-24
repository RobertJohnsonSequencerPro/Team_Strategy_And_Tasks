namespace TeamStrategyAndTasks.Core.DTOs;

/// <summary>Request to create a new node dependency.</summary>
public record AddDependencyRequest(
    NodeType      BlockerType,
    Guid          BlockerId,
    NodeType      BlockedType,
    Guid          BlockedId,
    DependencyType DependencyType = DependencyType.FinishToStart,
    string?       Notes          = null);

/// <summary>Read model returned by the service, with resolved titles for both sides.</summary>
public record NodeDependencyDto(
    Guid          Id,
    NodeType      BlockerType,
    Guid          BlockerId,
    string        BlockerTitle,
    NodeType      BlockedType,
    Guid          BlockedId,
    string        BlockedTitle,
    DependencyType DependencyType,
    string?       Notes);
