using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Core.Entities;

/// <summary>
/// A structured, documented strategic decision that may be linked to one or
/// more hierarchy nodes.  Decisions are distinct from comments (conversational)
/// and audit log entries (automated field-level changes).
/// </summary>
public class Decision : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Context { get; set; }
    public string? Rationale { get; set; }
    public string? AlternativesConsidered { get; set; }

    /// <summary>The user who made (or recorded) this decision.</summary>
    public Guid MadeById { get; set; }

    /// <summary>When the decision was actually made (may differ from CreatedAt).</summary>
    public DateTimeOffset MadeAt { get; set; }

    public DecisionStatus Status { get; set; } = DecisionStatus.Open;

    /// <summary>
    /// If this decision has been superseded, references the newer decision that
    /// replaced it.  Setting this automatically changes Status to Superseded.
    /// </summary>
    public Guid? SupersededById { get; set; }
    public Decision? SupersededBy { get; set; }

    public ICollection<DecisionNodeLink> NodeLinks { get; set; } = new List<DecisionNodeLink>();
}
