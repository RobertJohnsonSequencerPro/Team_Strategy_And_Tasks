using TeamStrategyAndTasks.Core.Entities;
using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Core.Interfaces;

public interface INotificationService
{
    Task<IReadOnlyList<Notification>> GetUnreadAsync(Guid userId, CancellationToken ct = default);
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default);
    Task MarkAllReadAsync(Guid userId, CancellationToken ct = default);
    Task MarkReadAsync(Guid notificationId, CancellationToken ct = default);
    Task CreateAsync(Guid userId, string message, NodeType? nodeType = null, Guid? nodeId = null, CancellationToken ct = default);
}
