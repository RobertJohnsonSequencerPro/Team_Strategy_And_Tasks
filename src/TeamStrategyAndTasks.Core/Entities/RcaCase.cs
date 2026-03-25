namespace TeamStrategyAndTasks.Core.Entities;

public class RcaCase : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? ProblemStatement { get; set; }
    public RcaType RcaType { get; set; } = RcaType.FiveWhys;
    public RcaCaseStatus Status { get; set; } = RcaCaseStatus.Drafting;

    /// <summary>Optional cross-module link to a CAPA case — ID only, no navigation property.</summary>
    public Guid? LinkedCapaCaseId { get; set; }

    /// <summary>Optional cross-module link to an audit finding — ID only, no navigation property.</summary>
    public Guid? LinkedFindingId { get; set; }

    public string? ProcessArea { get; set; }
    public string? PartFamily { get; set; }

    /// <summary>Written by the analyst on approval — the distilled root cause statement.</summary>
    public string? RootCauseSummary { get; set; }

    public Guid? InitiatedById { get; set; }
    public Guid? ApprovedById { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public bool IsArchived { get; set; }

    public ICollection<RcaCaseTag> Tags { get; set; } = [];
    public ICollection<FiveWhyNode> FiveWhyNodes { get; set; } = [];
    public ICollection<IshikawaCause> IshikawaCauses { get; set; } = [];
}
