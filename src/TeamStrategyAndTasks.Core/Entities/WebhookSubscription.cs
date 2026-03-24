using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Core.Entities;

/// <summary>
/// A registered HTTP endpoint that receives event notifications when strategy nodes change.
/// </summary>
public class WebhookSubscription : BaseEntity
{
    /// <summary>Human-readable label for this subscription.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Target URL to which the JSON payload is POSTed.</summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Comma-separated list of event types that trigger this webhook.
    /// Empty / "*" means all events.
    /// </summary>
    public string EventFilter { get; set; } = "*";

    /// <summary>
    /// Optional HMAC-SHA256 signing secret.  When set, each delivery includes an
    /// X-Webhook-Signature header so the receiver can verify authenticity.
    /// </summary>
    public string? Secret { get; set; }

    public bool IsActive { get; set; } = true;

    public Guid CreatedByUserId { get; set; }
}
