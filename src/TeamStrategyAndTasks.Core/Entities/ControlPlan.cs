using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Core.Entities;

/// <summary>
/// Root aggregate for a Control Plan — documents how process characteristics
/// are controlled, measured, and reacted to. Follows AIAG CP format.
/// </summary>
public class ControlPlan : BaseEntity
{
    public string Title { get; set; } = string.Empty;

    /// <summary>The process or product being controlled (e.g. "Weld Assembly Line 3").</summary>
    public string ProcessItem { get; set; } = string.Empty;

    public string? PartNumber { get; set; }
    public string? PartDescription { get; set; }

    /// <summary>Revision label, e.g. "Rev A", "Rev B".</summary>
    public string Revision { get; set; } = "Rev A";

    public ControlPlanStatus Status { get; set; } = ControlPlanStatus.Draft;

    public Guid? OwnerId { get; set; }

    /// <summary>Optional cross-module reference to a linked PFMEA study (link by ID only — no nav property).</summary>
    public Guid? LinkedPfmeaId { get; set; }

    public bool IsArchived { get; set; }

    public ICollection<ControlPlanCharacteristic> Characteristics { get; set; } = [];
    public ICollection<ControlPlanRevision> Revisions { get; set; } = [];
}
