namespace TeamStrategyAndTasks.Core.Entities;

/// <summary>
/// Root aggregate for a PFMEA study (Process Failure Mode and Effects Analysis).
/// A study targets a specific process/item and contains one or more failure mode rows.
/// </summary>
public class PfmeaRecord : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string ProcessItem { get; set; } = string.Empty;
    public string? Scope { get; set; }
    public string Revision { get; set; } = "Rev A";
    public Guid? OwnerId { get; set; }
    public bool IsArchived { get; set; }

    public ICollection<PfmeaFailureMode> FailureModes { get; set; } = [];
}
