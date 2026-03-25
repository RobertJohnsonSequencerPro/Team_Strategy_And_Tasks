using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Core.Entities;

/// <summary>
/// A recommended action tied to a specific failure mode.
/// Follows the PFMEA action loop: Open → InProgress → Closed → re-score failure mode.
/// </summary>
public class PfmeaAction : BaseEntity
{
    public Guid FailureModeId { get; set; }
    public PfmeaFailureMode FailureMode { get; set; } = null!;

    public string Description { get; set; } = string.Empty;
    public Guid? AssignedToId { get; set; }
    public DateTimeOffset? TargetDate { get; set; }
    public DateTimeOffset? CompletionDate { get; set; }
    public PfmeaActionStatus Status { get; set; } = PfmeaActionStatus.Open;

    /// <summary>Notes recorded when the action is closed (what was actually done).</summary>
    public string? OutcomeNotes { get; set; }

    public Guid CreatedById { get; set; }
}
