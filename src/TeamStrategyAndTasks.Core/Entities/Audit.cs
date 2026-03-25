namespace TeamStrategyAndTasks.Core.Entities;

public class Audit : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public AuditType AuditType { get; set; } = AuditType.Internal;
    public AuditStatus Status { get; set; } = AuditStatus.Planned;
    public string? Scope { get; set; }
    public Guid? LeadAuditorId { get; set; }
    public DateTimeOffset? ScheduledDate { get; set; }
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? CompletedDate { get; set; }
    public string? Notes { get; set; }
    public bool IsArchived { get; set; }

    public ICollection<AuditFinding> Findings { get; set; } = [];
}
