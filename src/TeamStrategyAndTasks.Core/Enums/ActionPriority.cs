namespace TeamStrategyAndTasks.Core.Enums;

/// <summary>
/// Computed action priority based on S/O/D scoring and RPN thresholds.
/// High: RPN >= 200 or Severity >= 9.
/// Medium: RPN >= 100 or (Severity >= 7 and Occurrence >= 4).
/// Low: all other cases.
/// </summary>
public enum ActionPriority
{
    Low,
    Medium,
    High
}
