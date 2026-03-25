namespace TeamStrategyAndTasks.Core.Entities;

/// <summary>
/// A single row in a PFMEA study — describes one failure mode for a process step
/// along with its pre-action and post-action S/O/D scores.
/// </summary>
public class PfmeaFailureMode : BaseEntity
{
    public Guid PfmeaId { get; set; }
    public PfmeaRecord Pfmea { get; set; } = null!;

    public int SortOrder { get; set; }

    /// <summary>Which process step this failure mode belongs to (e.g. "Step 3 – Weld bracket").</summary>
    public string ProcessStep { get; set; } = string.Empty;

    /// <summary>What the step is intended to accomplish (optional).</summary>
    public string? ProcessFunction { get; set; }

    /// <summary>The failure mode — what can go wrong (e.g. "Weld porosity").</summary>
    public string FailureDescription { get; set; } = string.Empty;

    /// <summary>Downstream effect of the failure on the customer/product.</summary>
    public string? PotentialEffect { get; set; }

    /// <summary>Root cause(s) of the failure.</summary>
    public string? PotentialCause { get; set; }

    /// <summary>Current prevention/detection controls in place.</summary>
    public string? CurrentControls { get; set; }

    // ── Pre-action scores (1 – 10) ──────────────────────────────────────────
    public int Severity { get; set; } = 5;
    public int Occurrence { get; set; } = 5;
    public int Detection { get; set; } = 5;

    // ── Post-action scores — null until re-scored after action closure ───────
    public int? SeverityAfter { get; set; }
    public int? OccurrenceAfter { get; set; }
    public int? DetectionAfter { get; set; }

    public Guid? AssignedToId { get; set; }
    public string? Notes { get; set; }

    public ICollection<PfmeaAction> Actions { get; set; } = [];
}
