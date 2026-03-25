using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Core.Entities;

/// <summary>
/// A piece of objective evidence linked to an AS9100 clause (document, record, link, or note).
/// </summary>
public class ClauseEvidenceItem : BaseEntity
{
    public Guid ClauseId { get; set; }
    public QualityClause Clause { get; set; } = null!;

    public EvidenceType EvidenceType { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>URL — populated when EvidenceType is Link.</summary>
    public string? Url { get; set; }

    public Guid AddedById { get; set; }
}
