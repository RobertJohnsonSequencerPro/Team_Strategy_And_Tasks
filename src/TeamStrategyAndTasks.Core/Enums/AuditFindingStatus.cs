namespace TeamStrategyAndTasks.Core.Enums;

/// <summary>
/// Lifecycle: Open → Containment → RootCauseAnalysis → CorrectiveAction →
///            EffectivenessVerification → Closed
/// </summary>
public enum AuditFindingStatus
{
    Open,
    Containment,
    RootCauseAnalysis,
    CorrectiveAction,
    EffectivenessVerification,
    Closed
}
