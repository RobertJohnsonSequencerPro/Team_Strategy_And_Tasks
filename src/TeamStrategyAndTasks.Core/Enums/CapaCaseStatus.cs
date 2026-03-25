namespace TeamStrategyAndTasks.Core.Enums;

/// <summary>
/// Lifecycle: Open → Containment → RootCauseAnalysis → CorrectiveAction →
///            EffectivenessVerification → Closed
/// </summary>
public enum CapaCaseStatus
{
    Open,
    Containment,
    RootCauseAnalysis,
    CorrectiveAction,
    EffectivenessVerification,
    Closed
}
