using TeamStrategyAndTasks.Core.Entities;
using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Core.Interfaces;

public interface ICommentService
{
    Task<IReadOnlyList<Comment>> GetForNodeAsync(NodeType nodeType, Guid nodeId, CancellationToken ct = default);

    Task<Comment> AddAsync(
        NodeType nodeType,
        Guid nodeId,
        Guid authorId,
        string body,
        Guid nodeOwnerId,
        Guid? parentCommentId = null,
        CancellationToken ct = default);

    Task DeleteAsync(Guid commentId, Guid requestingUserId, CancellationToken ct = default);
}
