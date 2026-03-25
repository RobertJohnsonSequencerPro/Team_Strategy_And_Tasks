namespace TeamStrategyAndTasks.Core.Entities;

public class AuditFinding : BaseEntity
{
    public Guid AuditId { get; set; }
    public Audit Audit { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public FindingType FindingType { get; set; } = FindingType.NonConformance;
    public FindingSeverity Severity { get; set; } = FindingSeverity.Minor;
    public string? ClauseReference { get; set; }
    public AuditFindingStatus Status { get; set; } = AuditFindingStatus.Open;
    public Guid? AssignedToId { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public string? ContainmentNotes { get; set; }
    public string? RootCauseNotes { get; set; }
    public string? CorrectiveActionNotes { get; set; }
    public string? Notes { get; set; }

    /// <summary>Cross-module reference to a CAPA case — ID only, no navigation property.</summary>
    public Guid? LinkedCapaCaseId { get; set; }
}
