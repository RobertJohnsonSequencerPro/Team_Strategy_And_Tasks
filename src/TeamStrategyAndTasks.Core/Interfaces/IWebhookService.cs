using TeamStrategyAndTasks.Core.DTOs;
using TeamStrategyAndTasks.Core.Entities;
using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Core.Interfaces;

public interface IWebhookService
{
    // ── Subscription management ───────────────────────────────────────────────
    Task<IReadOnlyList<WebhookSubscription>> GetAllAsync(CancellationToken ct = default);
    Task<WebhookSubscription> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<WebhookSubscription> CreateAsync(CreateWebhookSubscriptionRequest request, Guid userId, CancellationToken ct = default);
    Task<WebhookSubscription> UpdateAsync(Guid id, UpdateWebhookSubscriptionRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);

    // ── Event dispatch ────────────────────────────────────────────────────────
    /// <summary>
    /// Fires all active subscriptions whose EventFilter matches <paramref name="eventType"/>.
    /// Dispatched fire-and-forget so callers are never blocked.
    /// </summary>
    Task FireAsync(
        WebhookEventType eventType,
        NodeType nodeType,
        Guid nodeId,
        string nodeTitle,
        string? oldStatus,
        string? newStatus,
        Guid? changedByUserId,
        CancellationToken ct = default);
}
