namespace TeamStrategyAndTasks.Core.DTOs;

public record CreateWebhookSubscriptionRequest(
    string Description,
    string Url,
    string EventFilter = "*",
    string? Secret = null);

public record UpdateWebhookSubscriptionRequest(
    string Description,
    string Url,
    string EventFilter,
    string? Secret,
    bool IsActive);
