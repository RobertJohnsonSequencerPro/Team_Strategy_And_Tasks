using TeamStrategyAndTasks.Core.DTOs;
using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Core.Interfaces;

public interface IRiskService
{
    /// <summary>Returns all risks for a specific node, ordered by severity descending.</summary>
    Task<IReadOnlyList<NodeRiskDto>> GetForNodeAsync(NodeType nodeType, Guid nodeId, CancellationToken ct = default);

    /// <summary>Returns all Open risks across all nodes, ordered by severity descending.</summary>
    Task<IReadOnlyList<NodeRiskDto>> GetAllOpenAsync(CancellationToken ct = default);

    Task<NodeRiskDto> AddAsync(AddRiskRequest request, Guid performedByUserId, CancellationToken ct = default);

    Task<NodeRiskDto> UpdateAsync(Guid id, UpdateRiskRequest request, Guid performedByUserId, CancellationToken ct = default);

    Task DeleteAsync(Guid id, Guid performedByUserId, CancellationToken ct = default);
}
