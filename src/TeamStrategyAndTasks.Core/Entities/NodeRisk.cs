using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Core.Entities;

public class NodeRisk : BaseEntity
{
    /// <summary>The node type this risk is attached to.</summary>
    public NodeType NodeType { get; set; }

    /// <summary>The id of the node this risk is attached to.</summary>
    public Guid NodeId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public RiskLevel Probability { get; set; } = RiskLevel.Medium;
    public RiskLevel Impact      { get; set; } = RiskLevel.Medium;

    /// <summary>Computed severity: Probability × Impact (1–9). Max = High×High = 9.</summary>
    public int Severity => (int)Probability * (int)Impact;

    public string? MitigationPlan { get; set; }

    public Guid OwnerId { get; set; }

    public RiskStatus Status { get; set; } = RiskStatus.Open;

    public DateTimeOffset RaisedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? ResolvedAt { get; set; }
}
