namespace TeamStrategyAndTasks.Core.Entities;

public class CapaCase : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? CaseNumber { get; set; }
    public CapaType CapaType { get; set; } = CapaType.Corrective;
    public CapaCaseStatus Status { get; set; } = CapaCaseStatus.Open;
    public string? ProblemStatement { get; set; }
    public string? ContainmentActions { get; set; }
    public string? RootCauseAnalysis { get; set; }
    public string? ProposedCorrection { get; set; }
    public Guid? OwnerId { get; set; }
    public DateTimeOffset? TargetDate { get; set; }
    public DateTimeOffset? CompletedDate { get; set; }
    public bool IsArchived { get; set; }

    /// <summary>Cross-module reference to an audit finding — ID only, no navigation property.</summary>
    public Guid? LinkedFindingId { get; set; }

    public ICollection<CapaAction> Actions { get; set; } = [];
    public ICollection<EffectivenessCheck> EffectivenessChecks { get; set; } = [];
}
