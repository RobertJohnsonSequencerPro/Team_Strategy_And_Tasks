using TeamStrategyAndTasks.Core.Entities;
using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Core.Interfaces;

public interface IAuditService
{
    Task LogAsync(NodeType nodeType, Guid nodeId, Guid userId, string fieldName, string? oldValue, string? newValue, CancellationToken ct = default);
    Task<IReadOnlyList<AuditLog>> GetLogsForNodeAsync(NodeType nodeType, Guid nodeId, CancellationToken ct = default);
}
