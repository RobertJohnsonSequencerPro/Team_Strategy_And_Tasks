using TeamStrategyAndTasks.Core.DTOs;
using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Core.Interfaces;

public interface INodeDependencyService
{
    /// <summary>Returns all nodes that <paramref name="nodeId"/> is blocked by (its prerequisites).</summary>
    Task<IReadOnlyList<NodeDependencyDto>> GetBlockersForAsync(
        NodeType nodeType, Guid nodeId, CancellationToken ct = default);

    /// <summary>Returns all nodes that are blocked by <paramref name="nodeId"/> (downstream dependents).</summary>
    Task<IReadOnlyList<NodeDependencyDto>> GetBlockedByThisAsync(
        NodeType nodeType, Guid nodeId, CancellationToken ct = default);

    /// <summary>
    /// Adds a dependency. Validates no self-dependency, no duplicate, and no cycle.
    /// Throws <see cref="AppValidationException"/> on any violation.
    /// </summary>
    Task<NodeDependencyDto> AddAsync(
        AddDependencyRequest request, Guid performedByUserId, CancellationToken ct = default);

    Task RemoveAsync(Guid dependencyId, CancellationToken ct = default);

    /// <summary>
    /// Scans all active dependencies and sets any node whose blocker is not yet
    /// Done/Complete to <see cref="NodeStatus.Blocked"/> (if it is in an auto-managed status).
    /// Called after every dependency add/remove and after progress write-back.
    /// </summary>
    Task PropagateBlockedStatusAsync(CancellationToken ct = default);
}
