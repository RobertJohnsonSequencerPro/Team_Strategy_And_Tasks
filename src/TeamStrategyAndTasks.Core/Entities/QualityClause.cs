using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Core.Entities;

/// <summary>
/// Represents an AS9100D auditable clause (e.g. "4.1", "8.5").
/// Seeded with the standard clause library; administrators may add custom clauses.
/// Assessment state is denormalized onto the clause for efficient checklist queries.
/// </summary>
public class QualityClause : BaseEntity
{
    /// <summary>Clause identifier as printed in the standard (e.g. "4.1", "8.1.2").</summary>
    public string ClauseNumber { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>Controls list order on the checklist page.</summary>
    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    // ── Current assessment state (maintained by UpdateAssessmentAsync) ──────

    public ConformanceStatus ConformanceStatus { get; set; } = ConformanceStatus.NotAssessed;

    public Guid? AssignedToId { get; set; }

    public DateTimeOffset? ReviewDueDate { get; set; }

    public string? AssessmentNotes { get; set; }

    public DateTimeOffset? LastReviewedAt { get; set; }

    public Guid? LastReviewedById { get; set; }

    // ── Navigation ────────────────────────────────────────────────────────────

    public ICollection<ClauseEvidenceItem> EvidenceItems { get; set; } = [];
    public ICollection<ClauseReviewEvent> ReviewHistory { get; set; } = [];
}
