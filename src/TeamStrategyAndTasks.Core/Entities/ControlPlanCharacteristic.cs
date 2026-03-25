using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Core.Entities;

/// <summary>
/// A single row in a Control Plan — describes one process or product characteristic,
/// how it is controlled, measured, and what to do when it is out of spec.
/// </summary>
public class ControlPlanCharacteristic : BaseEntity
{
    public Guid ControlPlanId { get; set; }
    public ControlPlan ControlPlan { get; set; } = null!;

    public int SortOrder { get; set; }

    /// <summary>Which process step this characteristic belongs to (e.g. "Step 3 – Weld bracket").</summary>
    public string ProcessStep { get; set; } = string.Empty;

    /// <summary>Operation name or number (e.g. "OP-030").</summary>
    public string? ProcessOperation { get; set; }

    /// <summary>Characteristic identifier within this step (e.g. "3.1").</summary>
    public string? CharacteristicNo { get; set; }

    public CharacteristicType CharacteristicType { get; set; } = CharacteristicType.Process;

    /// <summary>Description of the characteristic being controlled (e.g. "Weld bead width").</summary>
    public string CharacteristicDescription { get; set; } = string.Empty;

    /// <summary>Specification or tolerance (e.g. "10.5 ± 0.1 mm").</summary>
    public string? SpecificationTolerance { get; set; }

    /// <summary>How the characteristic is controlled (prevention or detection method).</summary>
    public string? ControlMethod { get; set; }

    /// <summary>Sample size (e.g. "n = 5").</summary>
    public string? SamplingSize { get; set; }

    /// <summary>Sampling frequency (e.g. "Every 2 hours", "First piece").</summary>
    public string? SamplingFrequency { get; set; }

    /// <summary>Measurement technique or gage (e.g. "CMM", "Visual inspection", "Torque wrench").</summary>
    public string? MeasurementTechnique { get; set; }

    /// <summary>What the operator must do when the characteristic is found out of spec.</summary>
    public string? ReactionPlan { get; set; }

    public Guid? ResponsiblePersonId { get; set; }

    public string? Notes { get; set; }
}
