using Microsoft.EntityFrameworkCore;
using TeamStrategyAndTasks.Core.Entities;
using TeamStrategyAndTasks.Core.Enums;
using TeamStrategyAndTasks.Core.Interfaces;
using TeamStrategyAndTasks.Infrastructure.Data;

namespace TeamStrategyAndTasks.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _ctx;

    public NotificationService(AppDbContext ctx) => _ctx = ctx;

    public async Task<IReadOnlyList<Notification>> GetUnreadAsync(Guid userId, CancellationToken ct = default)
        => await _ctx.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .ToListAsync(ct);

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default)
        => await _ctx.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead, ct);

    public async Task MarkAllReadAsync(Guid userId, CancellationToken ct = default)
        => await _ctx.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), ct);

    public async Task MarkReadAsync(Guid notificationId, CancellationToken ct = default)
        => await _ctx.Notifications
            .Where(n => n.Id == notificationId)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), ct);

    public async Task CreateAsync(Guid userId, string message, NodeType? nodeType = null, Guid? nodeId = null, CancellationToken ct = default)
    {
        _ctx.Notifications.Add(new Notification
        {
            UserId = userId,
            Message = message,
            RelatedNodeType = nodeType,
            RelatedNodeId = nodeId
        });
        await _ctx.SaveChangesAsync(ct);
    }
}
