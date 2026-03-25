using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Core.Entities;

/// <summary>
/// Immutable record of a conformance status change on a quality clause.
/// Provides an audit-ready review history.
/// </summary>
public class ClauseReviewEvent : BaseEntity
{
    public Guid ClauseId { get; set; }
    public QualityClause Clause { get; set; } = null!;

    public ConformanceStatus PreviousStatus { get; set; }
    public ConformanceStatus NewStatus { get; set; }

    public Guid ReviewedById { get; set; }
    public DateTimeOffset ReviewedAt { get; set; } = DateTimeOffset.UtcNow;

    public string? Notes { get; set; }
}
