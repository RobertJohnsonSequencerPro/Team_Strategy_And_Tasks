using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Core.Interfaces;

public interface IDecisionService
{
    /// <summary>All decisions, most-recent first.</summary>
    Task<IReadOnlyList<DecisionDto>> GetAllAsync(CancellationToken ct = default);

    /// <summary>All decisions linked to a specific hierarchy node.</summary>
    Task<IReadOnlyList<DecisionDto>> GetForNodeAsync(NodeType nodeType, Guid nodeId, CancellationToken ct = default);

    Task<DecisionDto?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<DecisionDto> AddAsync(AddDecisionRequest request, Guid madeByUserId, CancellationToken ct = default);

    Task<DecisionDto> UpdateAsync(Guid id, UpdateDecisionRequest request, Guid performedByUserId, CancellationToken ct = default);

    /// <summary>
    /// Marks <paramref name="oldDecisionId"/> as Superseded and sets its
    /// SupersededById to <paramref name="newDecisionId"/>.
    /// </summary>
    Task SupersedeAsync(Guid oldDecisionId, Guid newDecisionId, Guid performedByUserId, CancellationToken ct = default);

    Task LinkNodeAsync(Guid decisionId, NodeType nodeType, Guid nodeId, CancellationToken ct = default);

    Task UnlinkNodeAsync(Guid decisionId, NodeType nodeType, Guid nodeId, CancellationToken ct = default);

    Task DeleteAsync(Guid id, Guid performedByUserId, CancellationToken ct = default);
}
