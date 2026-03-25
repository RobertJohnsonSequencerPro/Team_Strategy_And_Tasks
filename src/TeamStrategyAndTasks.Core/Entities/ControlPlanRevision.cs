using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Core.Entities;

/// <summary>
/// Immutable history record written whenever a Control Plan transitions status
/// (e.g. Draft → InReview → Approved) or a new revision supersedes an old one.
/// Once written, revisions are never deleted or modified.
/// </summary>
public class ControlPlanRevision : BaseEntity
{
    public Guid ControlPlanId { get; set; }
    public ControlPlan ControlPlan { get; set; } = null!;

    /// <summary>The revision label at the time of this transition (e.g. "Rev A").</summary>
    public string RevisionLabel { get; set; } = string.Empty;

    /// <summary>The status this transition moved the plan to.</summary>
    public ControlPlanStatus ToStatus { get; set; }

    /// <summary>Optional comment explaining the reason for the transition.</summary>
    public string? Comments { get; set; }

    public Guid ChangedById { get; set; }
    public DateTimeOffset ChangedAt { get; set; } = DateTimeOffset.UtcNow;
}
